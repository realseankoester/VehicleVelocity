using Microsoft.EntityFrameworkCore;
using VehicleVelocity.Common.Data;
using VehicleVelocity.Common.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using EFCore.NamingConventions;

namespace VehicleVelocity.Common.Data;


public class VehicleDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public VehicleDbContext(DbContextOptions<VehicleVelocity.Common.Data.VehicleDbContext> options) : base(options)
    {
    }

    // Update the empty constructor to call OnConfiguring properly
    public VehicleDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // 1. Direct, fail-safe connection string for local dev
            // This ensures that even if .env fails, the property is INITIALIZED.
            var connectionString = "Host=localhost;Port=5432;Database=inventory_db;Username=admin;Password=velocity123";

            optionsBuilder
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
                
            Console.WriteLine("[DEBUG] DbContext configured via internal fallback.");
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>().ToTable("vehicles");
        modelBuilder.Entity<Vehicle>().HasKey(v => v.Vin);
        base.OnModelCreating(modelBuilder);
    }
}