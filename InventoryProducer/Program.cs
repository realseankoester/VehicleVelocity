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

while (true)
{
    try 
    {
        Console.Write("\nEnter VIN (or 'auto' for simulation, 'exit' to quit): ");
        string input = Console.ReadLine() ?? "";

        if (input.ToLower() == "exit") break;
        if (string.IsNullOrWhiteSpace(input)) continue;

        // --- AUTOMATION MODE ---
        if (input.ToLower() == "auto")
        {
            Console.WriteLine("--- Starting Automated Simulation (20 Vehicles) ---");
            var random = new Random();
            
            for (int i = 0; i < 20; i++)
            {
                var autoVin = "VIN-" + random.Next(10000, 99999); 
                var autoLoc = "LOC-" + random.Next(1, 5).ToString("D2");
                
                // --- The Chaos Injector Logic ---
                string autoNotes;
                int chance = random.Next(1, 101);

                if (chance <= 12) // ~12% chance for a critical failure
                {
                    string[] criticals = { "Heavy rust on frame", "Active oil leak", "Corrosion detected", "Structural frame damage" };
                    autoNotes = criticals[random.Next(criticals.Length)];
                }
                else if (chance <= 30) // ~18% chance for minor issues
                {
                    string[] minors = { "Minor door ding", "Interior seat tear", "Scratched bumper", "Small rock chip" };
                    autoNotes = minors[random.Next(minors.Length)];
                }
                else
                {
                    autoNotes = "Automated sensor check: Pass";
                }

                var autoVehicle = new Vehicle
                {
                    Vin = autoVin.ToUpper(),
                    LocationID = autoLoc,
                    Mileage = random.Next(5000, 160000), // Range allows for "High Mileage" cliff
                    InspectionNotes = autoNotes,
                    DeploymentPhase = 2, // Assisted Mode
                    LastUpdated = DateTime.UtcNow
                };

                var autoJson = JsonSerializer.Serialize(autoVehicle);
                await producer.ProduceAsync(topic, new Message<Null, string> { Value = autoJson });
                
                Console.WriteLine($"[AUTO] Sent {autoVin} | Notes: {autoNotes}");
                await Task.Delay(400); // Slightly faster for smoother demo filling
            }
            Console.WriteLine("--- Simulation Complete. Dashboard Hydrated. ---");
            continue;
        }

        // --- MANUAL MODE ---
        // Only reach this if input wasn't "auto" or "exit"
        string manualVin = input;
        
        Console.Write("Enter Location ID (e.g., PHX-01): ");
        string manualLoc = Console.ReadLine() ?? "DEFAULT-01";
        if (string.IsNullOrWhiteSpace(manualLoc)) manualLoc = "DEFAULT-01";

        Console.Write("Enter Mileage: ");
        int.TryParse(Console.ReadLine(), out int manualMileage);

        Console.Write("Enter Inspection Notes: ");
        string manualNotes = Console.ReadLine() ?? "";

        // 4. Construct the Manual Vehicle Object
        var vehicle = new Vehicle
        {
            Vin = manualVin.ToUpper(),
            LocationID = manualLoc.ToUpper(), 
            Mileage = manualMileage,
            InspectionNotes = manualNotes,
            DeploymentPhase = 1, 
            LastUpdated = DateTime.UtcNow
        };

        // 5. Serialize and Send Manual Entry
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