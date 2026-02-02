namespace VehicleVelocity.Common.Services;
using VehicleVelocity.Common.Models;

public class InventoryService
{
    public bool NeedsSeniorInspection(Vehicle car)
    {
        return car.Mileage > 50000;
    }
}