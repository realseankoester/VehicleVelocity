using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleVelocity.Common.Models;
using VehicleVelocity.Common.Services;
using VehicleVelocity.Common.Data;
using Polly;
using Serilog;
using Serilog.Formatting.Compact;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// 1. Configuration & Environment Setup
DotNetEnv.Env.Load();
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")?.Trim().Replace("\r", "").Replace("\n", "");
var connectionString = $"Host=localhost;Database=inventory_db;Username=admin;Password={dbPassword}";

// 2. Structured Logging Setup
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(), "logs/audit_events.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("VehicleVelocity GateKeeper: Initializing Event Stream Listener");

// 3. Database & Kafka Configuration
var optionsBuilder = new DbContextOptionsBuilder<VehicleDbContext>();
optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

// Startup Migration (Run once)
using (var startupContext = new VehicleDbContext(optionsBuilder.Options))
{
    await startupContext.Database.MigrateAsync();
}

var config = new ConsumerConfig
{
    BootstrapServers = "127.0.0.1:9092",
    GroupId = "inventory-gatekeeper-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = true,
    SessionTimeoutMs = 45000
};

using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
consumer.Subscribe("inventory-updates");

// 4. Main Consumption Loop
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

try
{
    while (!cts.IsCancellationRequested)
    {
        try
        {
            // By passing the token here, it throws the exception we want to catch outside
            var result = consumer.Consume(cts.Token);
            if (result == null) continue;

            var car = JsonSerializer.Deserialize<Vehicle>(result.Message.Value);
            if (car != null)
            {
                await ProcessCar(car, optionsBuilder.Options);
            }
        }
        catch (ConsumeException e) when (e.Error.Code == ErrorCode.UnknownTopicOrPart)
        {
            Log.Information("Waiting for Kafka topic 'inventory-updates'...");
            // Use Task.Delay with the token to allow immediate exit if Ctrl+C is hit here
            await Task.Delay(5000, cts.Token);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException)) 
        {
            // This 'when' clause is the secret sauce. 
            // It prevents the "Operation Canceled" from being logged as a scary red Error.
            Log.Error(ex, "Error during consumption or processing");
            await Task.Delay(1000, cts.Token);
        }
    }
}
catch (OperationCanceledException) 
{ 
    Log.Information("GateKeeper: Shutdown signal received. Closing connection..."); 
}
finally 
{ 
    consumer.Close(); 
    Log.Information("GateKeeper: Connection closed. Goodbye!");
}

// 5. Audit & Persistence Logic
async Task ProcessCar(Vehicle car, DbContextOptions<VehicleDbContext> dbOptions)
{
    // Scouring & Scrubbing
    car.Vin = car.Vin?.ToUpper().Trim() ?? "UNKNOWN-VIN";
    car.InspectionNotes = car.InspectionNotes ?? "Clean/No notes";

    var auditService = new QualityAuditService();
    var aiVisionService = new ImageAnalysisService(); 

    // Perform Automated Audits
    var audit = await auditService.AnalyzeVehicleAsync(car);
    car.AIAuditNotes = await aiVisionService.AnalyzeImageAsync(car.ImageUrl ?? "");
    
    // Map Audit Results
    car.QualityScore = audit.QualityScore;
    car.PriorityLevel = audit.PriorityLevel;
    car.RiskReason = audit.RiskReason;
    car.IsHighPriorityAudit = audit.IsHighPriorityAudit; 

    if (car.DeploymentPhase == 2) // Assisted Mode logic
    {
        car.AuditRecommendation = car.IsHighPriorityAudit ? $"PRIORITY: {audit.RiskReason}" : "Standard Intake";
    }
    else // Passive Mode logic
    {
        car.IsHighPriorityAudit = false; 
        car.ShadowAction = $"AI Insight: {audit.RiskReason}";
    }

    // Logging
    if (car.IsHighPriorityAudit)
        Log.Warning("⚠️  [PRIORITY] VIN {Vin} | Score: {Score} | Reason: {Reason}", car.Vin, car.QualityScore, car.RiskReason);
    else
        Log.Information("✅ [PASS] VIN {Vin} | Score: {Score}", car.Vin, car.QualityScore);

    // Database Persistence with Polly
    var retryPolicy = Policy.Handle<Exception>()
        .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(2, i)));

    await retryPolicy.ExecuteAsync(async () =>
    {
        using var dbContext = new VehicleDbContext(dbOptions);
        var existing = await dbContext.Vehicles.FindAsync(car.Vin);
        if (existing == null) dbContext.Vehicles.Add(car);
        else dbContext.Entry(existing).CurrentValues.SetValues(car);
        await dbContext.SaveChangesAsync();
    });
}