using System.ComponentModel.DataAnnotations;
using System.Dynamic;

namespace VehicleVelocity.Common.Models;

public class Vehicle
{
    // --- 1. Primary Identifiers ---
    [Key]
    [Required]
    [StringLength(17, MinimumLength = 11)] 
    public string Vin { get; set; } = string.Empty;

    public string? Make { get; set; }
    public string? Model { get; set; }
    public int Year { get; set; }

    // --- 2. Input Data (From Producer) ---
    public int Mileage { get; set; }
    
    [MaxLength(1000)]
    public string InspectionNotes { get; set; } = string.Empty;
    
    public string? ImageUrl { get; set; }
    public string? LocationId { get; set; }

    // --- 3. System Configuration & AI Audit ---
    public int DeploymentPhase { get; set; } // 1 = Passive, 2 = Assisted
    public int QualityScore { get; set; }
    public int PriorityLevel { get; set; }
    public bool IsHighPriorityAudit { get; set; } // The "GateKeeper" Flag
    
    [MaxLength(500)]
    public string? AuditRecommendation { get; set; } // e.g., "PRIORITY: Corrosion"
    
    [MaxLength(500)]
    public string? ShadowAction { get; set; } // Used for Phase 1 analytics
    
    public string? AIAuditNotes { get; set; }
    public string? RiskReason { get; set; }

    // --- 4. Human-in-the-Loop Override ---
    public bool IsLeadOverride { get; set; } = false;
    public string? OverrideReason { get; set; }
    public string? ReviewedBy { get; set; }

    // --- 5. Persistence Logic ---
    private DateTime _lastUpdated = DateTime.UtcNow;
    public DateTime LastUpdated
    {
        get => _lastUpdated;
        set => _lastUpdated = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}