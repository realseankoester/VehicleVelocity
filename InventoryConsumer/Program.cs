using Confluent.Kafka;
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
using Microsoft.Extensions.AI; // Added: Brings in Microsoft's standard AI abstractions
using OllamaSharp;            // Added: Provides the concrete provider for your local Ollama engine

// ==========================================
// 1. CONFIGURATION & ENVIRONMENT SETUP
// ==========================================
DotNetEnv.Env.Load();

var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD");
var kafkaBroker = Environment.GetEnvironmentVariable("KAFKA_BROKER");

var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPass}";

// ==========================================
// 2. STRUCTURED LOGGING SETUP
// ==========================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(), "logs/audit_events.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("VehicleVelocity GateKeeper: Initializing Event Stream Listener");

// ==========================================
// 3. DATABASE & KAFKA INFRASTRUCTURE CONFIG
// ==========================================
var optionsBuilder = new DbContextOptionsBuilder<VehicleDbContext>();
optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

// DLQ Producer Setup
var dlqConfig = new ProducerConfig { BootstrapServers = kafkaBroker };
using var dlqProducer = new ProducerBuilder<Null, string>(dlqConfig).Build();

// Startup Migration (Self-healing database routine)
using (var startupContext = new VehicleDbContext(optionsBuilder.Options))
{
    await startupContext.Database.MigrateAsync();
}

// ==========================================
// 4. PIPELINE SERVICE LIFETIME INITIATION (The Crucial Addition)
// ==========================================
// Line-by-Line Breakdown of this new section:
// A. We create a single, persistent HTTP channel pointing to your Vivobook's local Ollama endpoint.
IChatClient ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "llama3.2");

// B. We pass that client directly into our semantic agent wrapper. It will reuse this reference indefinitely.
var semanticAiService = new VehicleSemanticAnalysisService(ollamaClient);

// C. We initialize our math and simulation blocks as long-lived, high-performance worker singletons.
var auditService = new QualityAuditService();
var aiVisionService = new ImageAnalysisService();

// ==========================================
// 5. KAFKA CONSUMER BOOTSTRAPPING
// ==========================================
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

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

string dlqTopic = "inventory-updates-failed";

// ==========================================
// 6. MAIN CONSUMPTION STREAM LOOP
// ==========================================
try
{
    while (!cts.IsCancellationRequested)
    {
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

        // INNER TRY-CATCH: The message execution safety net
        try
        {
            var car = JsonSerializer.Deserialize<Vehicle>(result.Message.Value);
            if (car != null)
            {
                // CRITICAL CORRECTION (Line 97 fixed): 
                // We now supply every single dependency required by the signature contract
                // in the exact order they are defined. Compiler error CS7036 is completely resolved.
                await ProcessCar(car, optionsBuilder.Options, auditService, aiVisionService, semanticAiService);
            }
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            Log.Error(ex, "CRITICAL: VIN processing failed. Shunting to Dead Letter Queue (DLQ).");

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

// ==========================================
// 7. AUDIT & PERSISTENCE ORCHESTRATION LOGIC
// ==========================================
async Task ProcessCar(
    Vehicle car, 
    DbContextOptions<VehicleDbContext> dbOptions,
    QualityAuditService auditor,
    ImageAnalysisService vision,
    IVehicleSemanticAnalysisService semanticAI) 
{
    // Sanitize inbound text values
    car.Vin = car.Vin?.ToUpperInvariant().Trim() ?? "UNKNOWN-VIN";
    car.InspectionNotes = string.IsNullOrWhiteSpace(car.InspectionNotes) ? "Clean/No notes" : car.InspectionNotes;
    car.LocationID = car.LocationID?.ToUpperInvariant().Trim() ?? "DEFAULT-01";

    // 1. Invoke your local Vivobook AI Agent to extract context semantically
    var aiInsight = await semanticAI.AnalyzeInspectionNotesAsync(car.InspectionNotes);
    
    // Save the text summary generated by Llama 3.2 into your data entity
    car.AIAuditNotes = aiInsight.ExtractionSummary;

    // 2. Pass that AI boolean result directly into the high-performance audit math engine
    var audit = auditor.AnalyzeVehicle(car, aiInsight.HasStructuralDamage);
    
    // 3. Map out the computed business fields
    car.QualityScore = audit.QualityScore;
    car.PriorityLevel = audit.PriorityLevel;
    car.RiskReason = audit.RiskReason;
    
    // Execute simulated vision processing
    car.AIAuditNotes += $" | [Vision]: {await vision.AnalyzeImageAsync(car.ImageUrl ?? "")}";

    // Manage Deployment Modes for the UI dashboard
    if (car.DeploymentPhase == DeploymentPhase.Assisted) 
    {
        car.AuditRecommendation = car.IsHighPriorityAudit 
            ? $"🚨 ACTION REQUIRED: {audit.RiskReason}" 
            : "✅ Proceed with Standard Intake";
        car.ShadowAction = "N/A - Active Mode";
    }
    else 
    {
        car.AuditRecommendation = "N/A - Passive Mode";
        car.ShadowAction = $"AI Insight: {audit.RiskReason}";
    }

    // Log the unified operational metrics
    if (car.IsHighPriorityAudit)
        Log.Warning("⚠️  [PRIORITY EVENT] VIN {Vin} flagged. Score: {Score} | Reason: {Reason}", car.Vin, car.QualityScore, car.RiskReason);
    else
        Log.Information("✅ [PASS] VIN {Vin} cleared. Score: {Score}", car.Vin, car.QualityScore);

    // 4. Persistence execution via resilient Polly retry framework
    var retryPolicy = Policy.Handle<Exception>()
        .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(2, i)));

    using var dbContext = new VehicleDbContext(dbOptions);

    await retryPolicy.ExecuteAsync(async () =>
    {
        var existing = await dbContext.Vehicles.FindAsync(car.Vin);
        if (existing == null) 
            dbContext.Vehicles.Add(car);
        else 
            dbContext.Entry(existing).CurrentValues.SetValues(car);
            
        await dbContext.SaveChangesAsync();
    });
}