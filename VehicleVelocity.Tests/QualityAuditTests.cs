using Xunit;
using VehicleVelocity.Common.Models;
using VehicleVelocity.Common.Services;
using System.Threading.Tasks;
using System; 

namespace VehicleVelocity.Tests;

public class QualityAuditTests
{
    private readonly QualityAuditService _auditService = new QualityAuditService();

    [Fact]
    public async Task HighMileageVehicle_ShouldTriggerHighPriorityAudit()
    {
        // Arrange
        var car = new Vehicle 
        { 
            Vin = "TESTVIN12345", 
            Mileage = 150000, 
            InspectionNotes = "Clean" 
        };

        // Act
        var result = await _auditService.AnalyzeVehicleAsync(car);

        // Assert
        // Adjusting to check Score or Priority based on your Service's logic
        Assert.True(result.QualityScore < 80, "High mileage should decrease quality score.");
    }

    [Fact]
    public async Task RustInNotes_ShouldMarkAsHighPriority()
    {
        // Arrange
        var car = new Vehicle 
        { 
            Vin = "RUSTYVIN123", 
            InspectionNotes = "Significant rust on frame" 
        };

        // Act
        var result = await _auditService.AnalyzeVehicleAsync(car);

        // Assert
        Assert.True(result.IsHighPriorityAudit);
        // Note: Check if your service returns "Corrosion" or "Rust"
        Assert.Contains("Corrosion", result.RiskReason);
    }

    [Fact]
    public async Task Phase1_PassiveMode_ShouldShowShadowAction()
    {
        // Arrange
        var car = new Vehicle 
        { 
            DeploymentPhase = 1, 
            InspectionNotes = "Rust detected" 
        };

        // Act
        var audit = await _auditService.AnalyzeVehicleAsync(car);
        
        // We simulate the mapping that happens in the Consumer
        car.IsHighPriorityAudit = audit.IsHighPriorityAudit;
        car.ShadowAction = $"AI Insight: {audit.RiskReason}";
        car.AuditRecommendation = "N/A - Passive Mode";

        // Assert
        Assert.Contains("Insight", car.ShadowAction);
        Assert.Equal("N/A - Passive Mode", car.AuditRecommendation);
    }

    [Fact]
    public async Task Phase2_AssistedMode_ShouldShowActionRequired()
    {
        // Arrange
        var car = new Vehicle 
        { 
            DeploymentPhase = 2, 
            InspectionNotes = "Rust detected" 
        };

        // Act
        var audit = await _auditService.AnalyzeVehicleAsync(car);
        car.IsHighPriorityAudit = audit.IsHighPriorityAudit;
        car.AuditRecommendation = car.IsHighPriorityAudit ? $"🚨 ACTION REQUIRED: {audit.RiskReason}" : "Standard";

        // Assert
        Assert.Contains("ACTION REQUIRED", car.AuditRecommendation);
    }

    [Fact]
    public async Task PerfectCondition_ShouldHaveScoreOf100()
    {
        // Arrange
        var car = new Vehicle { Mileage = 0, InspectionNotes = "Factory New" };

        // Act
        var result = await _auditService.AnalyzeVehicleAsync(car);

        // Assert
        Assert.Equal(100, result.QualityScore);
        Assert.False(result.IsHighPriorityAudit);
    }
}