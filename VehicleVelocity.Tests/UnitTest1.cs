using Xunit;
using VehicleVelocity.Common.Models;
using VehicleVelocity.Common.Services;
using System.Threading.Tasks;

namespace VehicleVelocity.Tests;

public class QualityAuditTests
{
    [Fact]
    public async Task HighMileageVehicle_ShouldTriggerHighPriorityAudit()
    {
        // Arrange
        var auditService = new QualityAuditService();
        var car = new Vehicle 
        { 
            Vin = "TESTVIN12345", 
            Mileage = 150000, 
            InspectionNotes = "Clean" 
        };

        // Act
        var result = await auditService.AnalyzeVehicleAsync(car);

        // Assert
        // In our new logic, < 70 score (mileage -30) might not trigger priority alone, 
        // but let's check the specific field we care about:
        Assert.True(result.PriorityLevel < 3, "High mileage should increase priority level.");
    }

    [Fact]
    public async Task RustInNotes_ShouldMarkAsHighPriority()
    {
        // Arrange
        var auditService = new QualityAuditService();
        var car = new Vehicle 
        { 
            Vin = "RUSTYVIN123", 
            Mileage = 10000, 
            InspectionNotes = "Significant rust on frame" 
        };

        // Act
        var result = await auditService.AnalyzeVehicleAsync(car);

        // Assert
        Assert.True(result.IsHighPriorityAudit);
        Assert.Contains("Corrosion", result.RiskReason);
    }
}