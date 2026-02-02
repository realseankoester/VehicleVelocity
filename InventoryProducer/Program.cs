using Confluent.Kafka;
using System.Text.Json;
using VehicleVelocity.Common.Models;

var config = new ProducerConfig 
{ 
    BootstrapServers = "127.0.0.1:9092",
    AllowAutoCreateTopics = true,
    SocketKeepaliveEnable = true,
    SocketTimeoutMs = 60000,
    MetadataMaxAgeMs = 180000
};

using var producer = new ProducerBuilder<Null, string>(config).Build();

Console.WriteLine("--- VehicleVelocity Inventory Input System ---");


while (true)
{
    Console.Write("\nEnter VIN (or type 'exit' to quit): ");
    string vin = Console.ReadLine() ?? "";
    if (vin.ToLower() == "eixt") break;

    Console.Write("Enter Mileage: ");
    if (!int.TryParse(Console.ReadLine(), out int mileage))
    {
        Console.WriteLine("Invalid mileage. Please enter a number.");
        continue;
    }

    Console.Write("Enter Status (e.g., Inspection, Repair, Ready): ");
    string status = Console.ReadLine() ?? "Unknown";

    var car = new Vehicle
    {
        Vin = vin,
        Status = status,
        Mileage = mileage,
        LastUpdated = DateTime.Now
    };

    string jsonString = JsonSerializer.Serialize(car);
    await producer.ProduceAsync("inventory-updates", new Message<Null, string> { Value = jsonString });

    Console.WriteLine($">>> Dispatched {vin} to Kafka.");

}