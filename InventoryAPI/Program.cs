using Microsoft.EntityFrameworkCore;
using VehicleVelocity.Common.Data;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(dbPassword))
{
    connectionString = connectionString.Replace("PLACEHOLDER", dbPassword);
}
else
{
    var missing = string.IsNullOrEmpty(connectionString) ? "ConnectionString" : "DB_PASSWORD";
    throw new Exception($"Database configuration error: {missing} is missing!");
}

builder.Services.AddDbContext<VehicleDbContext>(options =>
    options.UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();