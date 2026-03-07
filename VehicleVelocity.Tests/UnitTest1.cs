using Xunit;
using VehicleVelocity.Common.Models;
using VehicleVelocity.Common.Services;
using System.Threading.Tasks;

namespace VehicleVelocity.Tests;

public class UnitTest1
{
    [Fact]
public async Task AnalyzeVehicle_HighMileageAndRust_ReturnsManualReview()
{
    var service = new QualityAuditService();
    var car = new Vehicle 
    { 
        Vin = "TEST1", 
        Mileage = 130000, 
        InspectionNotes = "Some rust on the frame" // This drops score to 0
    }; 

    var result = await service.AnalyzeVehicleAsync(car);

    Assert.True(result.NeedsManualReview); // Score is 0, so 0 < 70 is TRUE
    Assert.Contains("High Mileage", result.RiskReason);
    Assert.Contains("Corrosion", result.RiskReason);
}
}