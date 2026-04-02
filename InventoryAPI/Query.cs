using VehicleVelocity.Common.Data;
using VehicleVelocity.Common.Models;
using Microsoft.EntityFrameworkCore;
using HotChocolate;

namespace InventoryAPI;

public class Query
{
    [UseFiltering]
    [UseSorting]
    [UseProjection]
    public IQueryable<Vehicle> GetVehicles([Service] VehicleDbContext context) 
    {
        
        return context.Vehicles.AsNoTracking();
      
    }
}