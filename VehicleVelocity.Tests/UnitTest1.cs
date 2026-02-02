using VehicleVelocity.Common.Models;
using VehicleVelocity.Common.Services;
using Xunit;

namespace VehicleVelocity.Tests;

public class InvenotoryTests
{
    [Fact]
    public void HighMileage_Should_RequireSeniorInspection()
    {
        var service = new InventoryService();
        var highMileageCar = new Vehicle { Vin = "TEST!", Mileage = 65000 };

        bool result = service.NeedsSeniorInspection(highMileageCar);

        Assert.True(result, "A car with 65k miles should return True for senior inspection.");
    }

    [Fact]
    public void LowMileage_Should_Not_RequireSeniorInspection()
    {
        var service = new InventoryService();
        var lowMileageCar = new Vehicle { Vin = "TEST2", Mileage = 10000 };

        bool result = service.NeedsSeniorInspection(lowMileageCar);

        Assert.False(result, "A car with 10k miles should return False for senior inspection.");
    }
}