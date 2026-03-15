using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Generic;
using System.IO;
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

// 1. Configuration & Environment Setup
DotNetEnv.Env.Load();
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")?.Trim().Replace("\r", "").Replace("\n", "");
var connectionString = $"Host=localhost;Database=inventory_db;Username=admin;Password={dbPassword}";

// 2. Structured Logging Setup (Serilog)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(), "logs/audit_events.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("VehicleVelocity GateKeeper: Initializing Event Stream Listener");

// 3. Database & Kafka Configuration
var optionsBuilder = new DbContextOptionsBuilder<VehicleDbContext>();
optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

Log.Information("Database: Checking schema and applying pending migrations...");
using (var startupContext = new VehicleDbContext(optionsBuilder.Options))
{
    await startupContext.Database.MigrateAsync();
}
Log.Information("Database: Schema is up to date.");

var config = new ConsumerConfig
{
    BootstrapServers = "127.0.0.1:9092",
    GroupId = "inventory-gatekeeper-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    SocketKeepaliveEnable = true,
};

using var consumer = new ConsumerBuilder<Confluent.Kafka.Ignore, string>(config).Build();
consumer.Subscribe("inventory-updates");

// 4. Ensure Topic Existence
using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = "localhost:9092" }).Build();
try {
    await adminClient.CreateTopicsAsync(new TopicSpecification[] { 
        new TopicSpecification { Name = "inventory-updates", ReplicationFactor = 1, NumPartitions = 1 } 
    });
}
catch (CreateTopicsException e)
{
    Log.Information("Topic Check: {Reason}", e.Results[0].Error.Reason);
}

// 5. Main Consumption Loop
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

Log.Information("--- GateKeeper Active: Monitoring inventory-updates topic ---");

try
{
    while (!cts.IsCancellationRequested)
    {
        try
        {
            var result = consumer.Consume(cts.Token);
            if (result == null) continue;

            var car = JsonSerializer.Deserialize<Vehicle>(result.Message.Value);
            if (car != null)
            {
                await ProcessCar(car, optionsBuilder.Options);
            }
        }
        catch (ConsumeException e)
        {
            Log.Error($"Stream error: {e.Error.Reason}");
            await Task.Delay(2000, cts.Token);
        }
    }
}
catch (OperationCanceledException) { /* Graceful shutdown */ }
finally { consumer.Close(); }

// 6. Audit & Persistence Logic
async Task ProcessCar(Vehicle car, DbContextOptions<VehicleDbContext> dbOptions)
{
    // Clean data before any logic happens
    car.Vin = car.Vin?.ToUpper().Trim() ?? "UNKNOWN-VIN";
    car.InspectionNotes = car.InspectionNotes ?? "Clean/No notes";
    car.ImageUrl = car.ImageUrl ?? "";

    // Check for deployment phase
    if (car.DeploymentPhase != 1 && car.DeploymentPhase != 2)
    {
        car.DeploymentPhase = 1; // Default to Passive/Shadow mode if value is wrong/missing
    }
    var auditService = new QualityAuditService();
    var aiVisionService = new ImageAnalysisService();

    // Perform Automated Audits
    var audit = await auditService.AnalyzeVehicleAsync(car);
    car.AIAuditNotes = await aiVisionService.AnalyzeImageAsync(car.ImageUrl ?? "");
    
    // Map Baseline Data
    car.QualityScore = audit.QualityScore;
    car.PriorityLevel = audit.PriorityLevel;
    car.RiskReason = audit.RiskReason;
    // Phase 1: Data collection - No impact on vehicle pipeline.
    // If the incoming message doesn't have a phase, we default to 1 (Shadow) for safety.
    int activeMode = car.DeploymentPhase;

if (activeMode == 1)
{
    car.IsHighPriorityAudit = false;
    car.ShadowAction = $"AI Insight: {audit.RiskReason}";
}
else if (activeMode == 2)
{
    if (audit.QualityScore < 70) 
    {
        car.IsHighPriorityAudit = true;
        car.AuditRecommendation = $"PRIORITY: {audit.RiskReason}";
    }
    else
    {
        car.IsHighPriorityAudit = false;
    }
}

if (car.IsHighPriorityAudit)
{
    Log.Warning("⚠️ [PRIORITY] VIN {Vin} | Score: {Score} | Reason: {Reason} | AI: {AiNotes}", 
        car.Vin, audit.QualityScore, audit.RiskReason, car.AIAuditNotes);
}
else
{
    string modeLabel = activeMode == 1 ? "PASSIVE" : "ASSISTED";
    Log.Information("✅ [{Mode}] VIN {Vin} | Score: {Score} | Clean Pass.", 
        modeLabel, car.Vin, audit.QualityScore);
}

    // Retry Policy for Database Resilience
    var retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    await retryPolicy.ExecuteAsync(async () =>
    {
        using var dbContext = new VehicleDbContext(dbOptions);
        await dbContext.Database.MigrateAsync();

        var existing = await dbContext.Vehicles.FindAsync(car.Vin);
        if (existing == null)
        {
            dbContext.Vehicles.Add(car);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(car);
        }
        await dbContext.SaveChangesAsync();
    });
}
