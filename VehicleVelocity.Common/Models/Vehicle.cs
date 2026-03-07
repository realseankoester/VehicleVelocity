using System.ComponentModel.DataAnnotations;

namespace VehicleVelocity.Common.Models;

public class Vehicle
{
    [Key]
    public string Vin { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string InspectionNotes { get; set; } = string.Empty;
    public string? RiskReason { get; set; }
    public string? ImageUrl { get; set; }
    public string? AIAuditNotes { get; set; } 

    private DateTime _lastUpdated = DateTime.UtcNow;
    public DateTime LastUpdated
    {
        get => _lastUpdated;
        set => _lastUpdated = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}