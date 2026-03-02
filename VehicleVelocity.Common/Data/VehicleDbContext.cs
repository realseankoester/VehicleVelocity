using Microsoft.EntityFrameworkCore;
using VehicleVelocity.Common.Data;
using VehicleVelocity.Common.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using EFCore.NamingConventions;

namespace VehicleVelocity.Common.Data;


public class VehicleDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public VehicleDbContext(DbContextOptions<VehicleVelocity.Common.Data.VehicleDbContext> options) : base(options)
    {
        
    }

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

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("--- Warning: DB_PASSWORD is null! ---");
            }
            var connectionString = $"Host=localhost;Database=inventory_db;Username=admin;Password={password}";

            optionsBuilder
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>().HasKey(v => v.Vin);
        base.OnModelCreating(modelBuilder);
    }
}