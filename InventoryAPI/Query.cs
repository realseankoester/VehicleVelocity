using VehicleVelocity.Common.Data;
using VehicleVelocity.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI;

public class Query
{
    [UseFiltering]
    [UseSorting]
    public IQueryable<Vehicle> GetVehicles(IDbContextFactory<VehicleDbContext> factory) 
    {
        
        var context = factory.CreateDbContext();
        return context.Vehicles;
    }
}