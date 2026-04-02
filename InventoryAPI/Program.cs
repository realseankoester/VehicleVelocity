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
    .AddProjections()
    .AddFiltering()
    .AddSorting();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
// Global Exception Handler
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        // Log the real error
        Console.WriteLine($"[GLOBAL ERROR]: {ex.Message}");
        
        // Return a clean message to the user/frontend
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { Error = "An internal service error occurred. Please try again later." });
    }
});

// 3. Configure Pipeline 
app.UseCors("OpenPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
}

app.MapControllers();

// 4. Map GraphQL Endpoint
app.MapGraphQL(); 

app.Run();