using Confluent.Kafka;
using System.Text.Json;
using VehicleVelocity.Common.Models;

var config = new ProducerConfig 
{ 
    BootstrapServers = "127.0.0.1:9092",
    AllowAutoCreateTopics = true,
    SocketKeepaliveEnable = true
};

using var producer = new ProducerBuilder<Null, string>(config).Build();

Console.WriteLine("--- 🛡️ VehicleVelocity Inventory Input System ---");

// 1. VIN Validation
string vin;
do {
    Console.Write("Enter VIN: ");
    vin = Console.ReadLine()?.ToUpper().Trim() ?? "";
} while (string.IsNullOrEmpty(vin));

// 2. Mileage Validation
int mileage;
string mileageInput;
do {
    Console.Write("Enter Mileage: ");
    mileageInput = Console.ReadLine() ?? "";
} while (!int.TryParse(mileageInput, out mileage) || mileage < 0);

// 3. Inspection Notes (CRITICAL: This drives the AI logic!)
Console.Write("Enter Inspection Notes (e.g. 'rust', 'cracked windshield', 'clean'): ");
string notes = Console.ReadLine() ?? "clean";

// 4. Deployment Phase
int phase;
do {
    Console.Write("Enter Deployment Phase (1=Passive, 2=Assisted): ");
} while (!int.TryParse(Console.ReadLine(), out phase) || (phase != 1 && phase != 2));

var car = new Vehicle
{
    Vin = vin,
    Mileage = mileage,
    InspectionNotes = notes,
    DeploymentPhase = phase,
    IsHighPriorityAudit = false, 
    ImageUrl = $"https://carvana.com/mock-storage/{notes.Replace(" ", "-")}.jpg",
    LastUpdated = DateTime.UtcNow
};

string jsonString = JsonSerializer.Serialize(car);
await producer.ProduceAsync("inventory-updates", new Message<Null, string> { Value = jsonString });

Console.WriteLine($"\n✅ SUCCESS: Dispatched {vin} to Kafka for { (phase == 1 ? "Passive Monitoring" : "Assisted Auditing") }.");