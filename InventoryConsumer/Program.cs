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
// Load Environment
DotNetEnv.Env.Load();

// Pull Variables
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD");
var kafkaBroker = Environment.GetEnvironmentVariable("KAFKA_BROKER");

// Build Connection String
var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPass}";

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

// DLQ Producer Setup
var dlqConfig = new ProducerConfig { BootstrapServers = kafkaBroker };
using var dlqProducer = new ProducerBuilder<Null, string>(dlqConfig).Build();

// Startup Migration (Run once)
using (var startupContext = new VehicleDbContext(optionsBuilder.Options))
{
    await startupContext.Database.MigrateAsync();
}

var config = new ConsumerConfig
{
    BootstrapServers = kafkaBroker,
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

string dlqTopic = "inventory-updates-failed";

try
{
    while (!cts.IsCancellationRequested)
    {
        // 1. Consume the message
        ConsumeResult<Ignore, string>? result = null;
        try
        {
            result = consumer.Consume(cts.Token);
            if (result == null) continue;
        }
        catch (ConsumeException e) when (e.Error.Code == ErrorCode.UnknownTopicOrPart)
        {
            Log.Information("Waiting for Kafka topic 'inventory-updates'...");
            await Task.Delay(5000, cts.Token);
            continue;
        }

        // 2. INNER TRY-CATCH: The "Safety Net" for individual car processing
        try
        {
            var car = JsonSerializer.Deserialize<Vehicle>(result.Message.Value);
            if (car != null)
            {
                // Scrubbing, Audit, and DB Save
                await ProcessCar(car, optionsBuilder.Options);
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            // If we are here, the DB save failed or the AI vision service crashed
            Log.Error(ex, "CRITICAL: VIN processing failed. Shunting to Dead Letter Queue (DLQ).");

            // Redirect the raw message to the failure topic
            var dlqMessage = new Message<Null, string> { Value = result.Message.Value };
            await dlqProducer.ProduceAsync(dlqTopic, dlqMessage);
            
            Log.Information("Message successfully shunted to {Topic}", dlqTopic);
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
    car.LocationID = car.LocationID?.ToUpper().Trim() ?? "DEFAULT-01";

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
        car.AuditRecommendation = car.IsHighPriorityAudit 
            ? $"🚨 ACTION REQUIRED: {audit.RiskReason}" 
            : "✅ Proceed with Standard Intake";
        car.ShadowAction = "N/A - Active Mode";
    }
    else // Passive Mode logic
    {
        car.AuditRecommendation = "N/A - Passive Mode";
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