using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleVelocity.Common.Data;
using VehicleVelocity.Common.Models;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleController : ControllerBase
{
    private readonly VehicleDbContext _context;
    
    public VehicleController(VehicleDbContext context)
    {
        _context = context;
    }

    // GET: api/vehicle
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles()
    {
        // Added .AsNoTracking() to optimize read throughput and memory allocations
        return await _context.Vehicles.AsNoTracking().ToListAsync();
    }

    // GET: api/vehicle/{vin}
    [HttpGet("{vin}")]
    public async Task<ActionResult<Vehicle>> GetVehicle(string vin)
    {
        // FindAsync checks local tracking state first. Since we are read-only, 
        // SingleOrDefaultAsync with AsNoTracking is highly performant for explicit string keys.
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .SingleOrDefaultAsync(v => v.Vin == vin.ToUpperInvariant());

        if (vehicle == null)
        {
            return NotFound(new { Message = $"Vehicle with VIN {vin} not found." });
        }

        return vehicle;
    }
}