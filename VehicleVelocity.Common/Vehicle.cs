namespace VehicleVelocity.Common.Models;

public class Vehicle
{
    public string Vin { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}