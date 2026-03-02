using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.IO;
using VehicleVelocity.Common.Models;
using VehicleVelocity.Common.Services;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VehicleVelocity.Common.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Polly;
using Polly.Retry;
using Serilog;
using Serilog.Formatting.Compact;
using Microsoft.VisualBasic;

// 1. Force a clean load
DotNetEnv.Env.Load();
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")?.Trim().Replace("\r", "").Replace("\n", "");

// 2. Direct String Construction (Avoids .Replace issues)
var connectionString = $"Host=localhost;Database=inventory_db;Username=admin;Password={dbPassword}";

// 3. Debug - Check if we are sending the literal word "PLACEHOLDER"
if (connectionString.Contains("PLACEHOLDER")) 
{
    Console.WriteLine("CRITICAL ERROR: Password was not replaced!");
}

Console.WriteLine($"DEBUG: Connection String built. Length: {connectionString.Length}");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(), "logs/audit_events.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("Inventory GateKeeper starting up...");

var optionsBuilder = new DbContextOptionsBuilder<VehicleDbContext>();
optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

var config = new ConsumerConfig
{
    BootstrapServers = "127.0.0.1:9092",
    GroupId = "inventory-gatekeeper-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    SocketKeepaliveEnable = true,
    SocketTimeoutMs = 60000,
    MetadataMaxAgeMs = 180000
};

using var consumer = new ConsumerBuilder<Confluent.Kafka.Ignore, string>(config).Build();
consumer.Subscribe("inventory-updates");

Console.WriteLine("--- Inventory GateKeeper: Monitoring Stream ---");

using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = "localhost:9092" }).Build();

try {
    await adminClient.CreateTopicsAsync(new TopicSpecification[] { 
        new TopicSpecification { Name = "inventory-updates", ReplicationFactor = 1, NumPartitions = 1 } 
    });
    Console.WriteLine("Topic 'inventory-updates' created successfully.");
}
catch (CreateTopicsException e) {
    Console.WriteLine($"Topic might already exist: {e.Results[0].Error.Reason}");
}

while (true)
{
    try
    {
        // Add a small timeout (e.g., 1 second) so it doesn't block forever if the topic is still warming up
        var result = consumer.Consume(TimeSpan.FromSeconds(1));

        if (result == null) continue; // No message yet, loop back and try again

        string jsonString = result.Message.Value;
        var car = JsonSerializer.Deserialize<Vehicle>(jsonString);

        if (car != null)
        {
            ProcessCar(car).GetAwaiter().GetResult();
        }
    }
    catch (ConsumeException e) when (e.Error.Code == ErrorCode.UnknownTopicOrPart)
    {
        // This is exactly what's happening now. We just wait.
        Console.WriteLine("Topic is still initializing... waiting 2 seconds.");
        Thread.Sleep(2000);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

async Task ProcessCar(Vehicle car)
{
    // Initialize new Audit Service
    var auditService = new QualityAuditService();

    var audit = await auditService.AnalyzeVehicleAsync(car);

    var retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, timeSpan, retryCount, context) =>
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[RETRY] Database busy. Attempt {retryCount}. Waiting {timeSpan.TotalSeconds}s...");
                Console.ResetColor();
            });

    await retryPolicy.ExecuteAsync(async () =>
    {
        using var dbContext = new VehicleDbContext(optionsBuilder.Options);
        await dbContext.Database.EnsureCreatedAsync();

        var existingVehicle = await dbContext.Vehicles.FindAsync(car.Vin);
        if (existingVehicle == null)
        {
            dbContext.Vehicles.Add(car);
        }
        else
        {
            existingVehicle.Mileage = car.Mileage;
            existingVehicle.Status = car.Status;
            existingVehicle.InspectionNotes = car.InspectionNotes;
            existingVehicle.LastUpdated = car.LastUpdated;
        }
        await dbContext.SaveChangesAsync();
    });

    bool highMileage = car.Mileage > 120000;

    if (audit.NeedsManualReview || highMileage)
    {
        string reason = audit.NeedsManualReview ? audit.RiskReason : "High Mileage";

        Log.Warning("Audit Flagged: {Vin}. Reason: {Reason}. Score {Score}. Data: {@Vehicle}",
            car.Vin, reason, audit.QualityScore, car);
    } 
    else
    {
        Log.Information("Auto-Pass: {Vin} cleared quality gates.", car.Vin);  
    }
    
}
