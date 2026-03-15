using Microsoft.EntityFrameworkCore;
using VehicleVelocity.Common.Models;

namespace VehicleVelocity.Common.Data;

public class VehicleDbContext : DbContext
{
    public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options)
    {
    }

    // Default constructor for EF Core Migrations
    public VehicleDbContext()
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            DotNetEnv.Env.TraversePath().Load();
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            // Fallback for local development if Env fails
            var connectionString = $"Host=localhost;Database=inventory_db;Username=admin;Password={password}";

            optionsBuilder
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention(); // Keeps Postgres happy with snake_case
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vehicle>(entity =>
        {
            // Explicitly set the table name
            entity.ToTable("vehicles");

            // VIN is our Primary Key
            entity.HasKey(v => v.Vin);

            // Ensure our text fields don't default to 'unlimited' if we have specific lengths
            entity.Property(v => v.Vin).HasMaxLength(17).IsRequired();
            
            // You can add index for faster lookups in the demo
            entity.HasIndex(v => v.IsHighPriorityAudit);
        });
    }
}