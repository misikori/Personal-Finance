using Microsoft.AspNetCore.Builder;
using Serilog;

namespace MarketGateway.StartUp;

public static class LoggingExtensions
{
    public static void AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration) 
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}