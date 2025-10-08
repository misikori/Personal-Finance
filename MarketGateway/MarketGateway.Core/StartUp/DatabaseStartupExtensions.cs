using MarketGateway.Data;
using MarketGateway.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MarketGateway.StartUp;

public static class DatabaseStartupExtensions
{
    public static async Task InitDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        // Get configured storage options
        var storageOptions = scope.ServiceProvider.GetRequiredService<IOptions<StorageOptions>>().Value;
        
        // Create necessary directories from configuration
        if (!string.IsNullOrEmpty(storageOptions.RootDirectory))
        {
            Directory.CreateDirectory(storageOptions.RootDirectory);
            
            // Create logs directory if needed
            var logsDir = Path.Combine(storageOptions.RootDirectory, "logs");
            Directory.CreateDirectory(logsDir);
        }
        
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var db  = scope.ServiceProvider.GetRequiredService<MarketDbContext>();
        
        await DebugEfSqliteAsync(db, scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DBDebug"));

        if (env.IsDevelopment())
        {
            await db.Database.EnsureCreatedAsync();
        }
        else
        {
            await db.Database.MigrateAsync();
        }
    }


    private static async Task DebugEfSqliteAsync(MarketDbContext db, ILogger logger)
    {
        var conn = (SqliteConnection)db.Database.GetDbConnection();
        logger.LogInformation("SQLite connection string: {Conn}", conn.ConnectionString);
        logger.LogInformation("SQLite file exists: {Exists}", File.Exists(conn.DataSource));
        
        await db.Database.OpenConnectionAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
            using var rdr = await cmd.ExecuteReaderAsync();
            var tables = new List<string>();
            while (await rdr.ReadAsync()) tables.Add(rdr.GetString(0));
            logger.LogInformation("SQLite tables: {Tables}", string.Join(", ", tables));
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

}
