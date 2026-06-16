using Microsoft.EntityFrameworkCore;
using VehicleVelocity.Common.Models;
using System;

namespace VehicleVelocity.Common.Data;

public class VehicleDbContext : DbContext
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    // Cleaned up the generic type footprint
    public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options)
    {
    }

    public VehicleDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Fail-safe connection string for local migrations / design-time execution
            var connectionString = "Host=localhost;Port=5432;Database=inventory_db;Username=admin;Password=velocity123";

            optionsBuilder
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
                
            Console.WriteLine("[DEBUG] DbContext configured via internal fallback.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API can remain clear since Data Annotations and Npgsql plugins 
        // are handling the key registration and snake_case mapping automatically.
        base.OnModelCreating(modelBuilder);
    }
}