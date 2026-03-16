using System.IO;
using Microsoft.EntityFrameworkCore;
using VehicleVelocity.Common.Data;
using InventoryAPI;
using Microsoft.Extensions.Configuration; // Add this
using InventoryAPI.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Direct injection from our helper
var connectionString = DbConfig.GetConnectionString();

builder.Services.AddPooledDbContextFactory<VehicleDbContext>(options =>
    options.UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention());

// 2. Add GraphQL Services
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddFiltering()
    .AddSorting();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Configure Pipeline (AFTER builder.Build())
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// 4. Map GraphQL Endpoint
app.MapGraphQL(); 

app.Run();