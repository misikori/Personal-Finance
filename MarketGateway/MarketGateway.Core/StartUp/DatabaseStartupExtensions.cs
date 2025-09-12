using MarketGateway.Data;
using MarketGateway.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MarketGateway.StartUp;

public static class DatabaseStartupExtensions
{
    public static async Task InitDatabaseAsync(this WebApplication app)
    {
        Directory.CreateDirectory(Path.Combine("..", "MarketGateway.Data", "DataStorage"));
        Directory.CreateDirectory(Path.Combine("..", "MarketGateway.Data", "logs"));
        
        using var scope = app.Services.CreateScope();
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
    
    
    public static async Task DebugEfSqliteAsync(MarketDbContext db, ILogger logger)
    {
        // 1) Where are we connected?
        var conn = (SqliteConnection)db.Database.GetDbConnection();
        logger.LogInformation("SQLite connection string: {Conn}", conn.ConnectionString);
        logger.LogInformation("SQLite file exists: {Exists}", File.Exists(conn.DataSource));

        // 2) What table name does EF think for QuoteEntity?
        var et = db.Model.FindEntityType(typeof(QuoteEntity));
        var efTable = et?.GetTableName();
        logger.LogInformation("EF mapped table for QuoteEntity: {Table}", efTable);

        // 3) What tables actually exist in the file?
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