using Confluent.Kafka;
using System;
using System.Runtime.Serialization;
using System.Threading;
using VehicleVelocity.Common.Models;
using VehicleVelocity.Common.Services;
using System.Text.Json;
using System.ComponentModel.Design;

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

Console.WriteLine("--- Invenotory GateKeeper: Monitoring Stream ---");

while (true)
{
    var result = consumer.Consume(CancellationToken.None);
    string jsonString = result.Message.Value;
    try
    {
        var car = JsonSerializer.Deserialize<Vehicle>(result.Message.Value);

        if (car != null)
        {
            ProcessCar(car);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

void ProcessCar(Vehicle car)
{
    var service = new InventoryService();
    if (service.NeedsSeniorInspection(car))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        string alertMsg = $"[ALERT] {DateTime.Now}: VIN {car.Vin} needs Senior Inspection ({car.Mileage} miles).";
        Console.WriteLine(alertMsg);
        Console.ResetColor();

        LogToAuditFile(alertMsg);
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[PASS] VIN {car.Vin} is clear.");
        Console.ResetColor();
    }
}

void LogToAuditFile(string Message)
{
    using (StreamWriter sw = File.AppendText("audit_log.txt"))
    {
        sw.WriteLine(Message);
    }
}