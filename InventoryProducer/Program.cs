using Confluent.Kafka;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using VehicleVelocity.Common.Models;

// ==========================================
// 1. KAFKA BROKER CLUSTER CONFIGURATION
// ==========================================
var config = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    Acks = Acks.All, // Guarantees that the "GateKeeper" consumer acknowledges data persistence
    MessageSendMaxRetries = 3,
    RetryBackoffMs = 500
};

using var producer = new ProducerBuilder<Null, string>(config).Build();
string topic = "inventory-updates";

Console.WriteLine("--- VehicleVelocity: Intake Terminal (Producer) ---");

while (true)
{
    try 
    {
        Console.Write("\nEnter VIN (or 'auto' for simulation, 'exit' to quit): ");
        string input = Console.ReadLine() ?? "";

        if (input.ToLower() == "exit") break;
        if (string.IsNullOrWhiteSpace(input)) continue;

        // ==========================================
        // 2. AUTOMATION & CHAOS SIMULATION MODE
        // ==========================================
        if (input.ToLower() == "auto")
        {
            Console.WriteLine("--- Starting Automated Simulation (20 Vehicles) ---");
            
            for (int i = 0; i < 20; i++)
            {
                // FIX 1: Accessing Random.Shared statically and using standard string format padding (.ToString("D2"))
                var autoVin = "VIN-" + Random.Shared.Next(10000, 99999); 
                var autoLoc = "LOC-" + Random.Shared.Next(1, 5).ToString("D2");
                
                // --- The Chaos Injector Logic ---
                string autoNotes; 
                int chance = Random.Shared.Next(1, 101);

                // FIX 2: Removed the duplicate 'string' type prefix. This is now a clean variable assignment.
                autoNotes = chance switch
                {
                    <= 12 => new[] { "Heavy rust on frame", "Active oil leak", "Corrosion detected", "Structural frame damage" }[Random.Shared.Next(4)],
                    <= 30 => new[] { "Minor door ding", "Interior seat tear", "Scratched bumper", "Small rock chip" }[Random.Shared.Next(4)],
                    _     => "Automated sensor check: Pass"
                };

                var autoVehicle = new Vehicle
                {
                    Vin = autoVin.ToUpper(),
                    LocationID = autoLoc,
                    Mileage = Random.Shared.Next(5000, 160000), // Using high-performance static random
                    InspectionNotes = autoNotes,
                    DeploymentPhase = DeploymentPhase.Assisted, // FIX 3: Replaced the raw integer '2' with explicit strong Enum typing
                    LastUpdated = DateTime.UtcNow
                };

                var autoJson = JsonSerializer.Serialize(autoVehicle);
                await producer.ProduceAsync(topic, new Message<Null, string> { Value = autoJson });
                
                Console.WriteLine($"[AUTO] Sent {autoVin} | Notes: {autoNotes}");
                await Task.Delay(400);
            }
            Console.WriteLine("--- Simulation Complete. Dashboard Hydrated. ---");
            continue;
        }

        // ==========================================
        // 3. MANUAL INTAKE MODE
        // ==========================================
        string manualVin = input;
        
        Console.Write("Enter Location ID (e.g., PHX-01): ");
        string manualLoc = Console.ReadLine() ?? "DEFAULT-01";
        if (string.IsNullOrWhiteSpace(manualLoc)) manualLoc = "DEFAULT-01";

        Console.Write("Enter Mileage: ");
        int.TryParse(Console.ReadLine(), out int manualMileage);

        Console.Write("Enter Inspection Notes: ");
        string manualNotes = Console.ReadLine() ?? "";

        var vehicle = new Vehicle
        {
            Vin = manualVin.ToUpper(),
            LocationID = manualLoc.ToUpper(), 
            Mileage = manualMileage,
            InspectionNotes = manualNotes,
            DeploymentPhase = DeploymentPhase.Passive, // FIX 3: Replaced the raw integer '1' with explicit strong Enum typing
            LastUpdated = DateTime.UtcNow
        };

        var messageValue = JsonSerializer.Serialize(vehicle);
        await producer.ProduceAsync(topic, new Message<Null, string> { Value = messageValue });

        Console.WriteLine($"[SUCCESS] {vehicle.Vin} transmitted from {manualLoc}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Pipeline disruption: {ex.Message}");
    }
}

Console.WriteLine("Intake Terminal Closed.");