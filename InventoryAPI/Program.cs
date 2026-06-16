using System;
using Microsoft.EntityFrameworkCore;
using VehicleVelocity.Common.Data;
using InventoryAPI;
using InventoryAPI.Helpers;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Optimization Configuration
var connectionString = DbConfig.GetConnectionString();

builder.Services.AddPooledDbContextFactory<VehicleDbContext>(options =>
    options.UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention());

// 2. Add GraphQL Services (With HotChocolate Engine)
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddProjections() // Automatically optimizes database SELECT queries based on GraphQL fields requested
    .AddFiltering()   // Enables complex GraphQL WHERE filtering clauses automatically
    .AddSorting();    // Enables complex GraphQL ORDER BY clauses automatically

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

// 3. Global Exception Handler (Kept neat inline, but gracefully logging)
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GLOBAL REST ERROR]: {ex.Message}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { Error = "An internal service error occurred. Please try again later." });
    }
});

app.UseCors("OpenPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Map your GraphQL UI dashboard (Banana Cake Pop) and routing endpoints
app.MapGraphQL(); 

app.Run();