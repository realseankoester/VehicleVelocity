namespace InventoryAPI.Helpers;

public static class DbConfig
{
    public static string GetConnectionString()
    {
        // Explicitly load .env from the API's root folder
        DotNetEnv.Env.Load();
        
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "velocity123";
        
        // We use the full connection string here to bypass appsettings issues entirely
        return $"Host=localhost;Port=5432;Database=inventory_db;Username=admin;Password={password}";
    }
}