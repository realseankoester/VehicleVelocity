using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleVelocity.Common.Data;
using VehicleVelocity.Common.Models;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleController: ControllerBase
{
    private readonly VehicleDbContext _context;
    
    public VehicleController(VehicleDbContext context)
    {
        _context = context;
    }

    // GET: api/vehicles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles()
    {
        return await _context.Vehicles.ToListAsync();
    }

    // Get: api/vehicles/{vin}
    [HttpGet("{vin}")]
    public async Task<ActionResult<Vehicle>> GetVehicle(string vin)
    {
        var vehicle = await _context.Vehicles.FindAsync(vin);

        if (vehicle == null)
        {
            return NotFound();
        }

        return vehicle;
    }
}