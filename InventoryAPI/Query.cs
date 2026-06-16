using System.Linq;
using Microsoft.EntityFrameworkCore;
using VehicleVelocity.Common.Data;
using VehicleVelocity.Common.Models;
using HotChocolate;
using HotChocolate.Data;

namespace InventoryAPI;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    // Design Decision: We inject the Factory instead of a raw context. 
    // HotChocolate will inject the pooled factory instance automatically.
    public IQueryable<Vehicle> GetVehicles(IDbContextFactory<VehicleDbContext> contextFactory) 
    {
        // We create the context instance explicitly. 
        // HotChocolate manages the IQueryable deferral safely.
        var context = contextFactory.CreateDbContext();
        
        return context.Vehicles.AsNoTracking();
    }
}