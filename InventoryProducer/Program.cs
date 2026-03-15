using Confluent.Kafka;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VehicleVelocity.Common.Models;

// 1. Kafka Configuration
var config = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    Acks = Acks.All, // Ensure "GateKeeper" receives the message
    MessageSendMaxRetries = 3,
    RetryBackoffMs = 500
};

using var producer = new ProducerBuilder<Null, string>(config).Build();
string topic = "inventory-updates";

Console.WriteLine("--- VehicleVelocity: Intake Terminal (Producer) ---");
Console.WriteLine("Direct Entry Mode: Enter VIN to begin or 'exit' to quit.");

while (true)
{
    try 
    {
        // 2. Direct VIN Prompt (with Exit check)
        Console.Write("\nEnter VIN (or 'exit'): ");
        var vin = Console.ReadLine() ?? "";

        if (vin.ToLower() == "exit") break;
        if (string.IsNullOrWhiteSpace(vin)) continue;

        // 3. Capture Remaining Intake Data
        Console.Write("Enter Mileage: ");
        int.TryParse(Console.ReadLine(), out int mileage);

        Console.Write("Enter Inspection Notes (e.g., 'rust', 'rip', 'clean'): ");
        var notes = Console.ReadLine() ?? "";

        Console.Write("Deployment Phase (1=Passive, 2=Assisted): ");
        int.TryParse(Console.ReadLine(), out int phase);

        // 4. Construct the Vehicle Object
        var vehicle = new Vehicle
        {
            Vin = vin.ToUpper(), // Standardize to uppercase
            Mileage = mileage,
            InspectionNotes = notes,
            DeploymentPhase = (phase == 2) ? 2 : 1, 
            LastUpdated = DateTime.UtcNow
        };

        // 5. Serialize and Send to Kafka
        var messageValue = JsonSerializer.Serialize(vehicle);
        var deliveryResult = await producer.ProduceAsync(topic, new Message<Null, string> { Value = messageValue });

        Console.WriteLine($"[SUCCESS] {vehicle.Vin} sent to {deliveryResult.TopicPartitionOffset}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Pipeline disruption: {ex.Message}");
    }
}

Console.WriteLine("Intake Terminal Closed.");