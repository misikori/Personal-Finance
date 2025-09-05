using System.Net;
using MarketGateway.Application;
using MarketGateway.Data;
using MarketGateway.Data.Interfaces;
using MarketGateway.Data.Services;
using MarketGateway.Interfaces;
using MarketGateway.Mapping;
using MarketGateway.Providers.Configuration;
using MarketGateway.Providers.Interfaces;
using MarketGateway.Providers.Parsing;
using MarketGateway.Providers.Providers;
using Microsoft.EntityFrameworkCore;

namespace MarketGateway.StartUp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarketGateway(
        this IServiceCollection services, IConfiguration config)
    {
        
        services.AddSingleton<IVendorResponseParser, VendorResponseParser>();
        services.AddSingleton(TimeProvider.System);

        
        
        services.AddAutoMapper(typeof(ProtoMappingProfile).Assembly);


        var vendorsPath = config["Vendors:Folder"]
                          ?? "../MarketGateway.Providers/Configuration/Vendors";
        var vendorConfigs = VendorConfigLoader.LoadFromFolder(vendorsPath);
        services.AddSingleton(vendorConfigs);
        
        foreach (var conf in vendorConfigs)
        {
            var clientName = $"vendor:{conf.Vendor}";
            services.AddHttpClient(clientName, client =>
            {
                client.BaseAddress = new Uri(conf.BaseUrl, UriKind.Absolute);
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                client.Timeout = TimeSpan.FromSeconds(10);
            }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression =
                    DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            });
            
            services.AddScoped<IMarketDataProvider>(sp =>
            {
                var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
                var usageTracker = sp.GetRequiredService<IApiUsageTracker>();
                var parser      = sp.GetRequiredService<IVendorResponseParser>();
                var logger = sp.GetRequiredService<ILogger<VendorMarketDataProvider>>();

                return new VendorMarketDataProvider(conf, httpFactory, usageTracker,parser, logger);
            });
        }
        
        services.AddScoped<IMarketDataProviderResolver, MarketDataProviderResolver>();
        services.AddScoped<IMarketDataBroker, MarketDataBroker>();
        services.AddScoped<IApiUsageTracker, ApiUsageTracker>();
        services.AddDbContextPool<MarketDbContext>(options =>
        {
            options.UseSqlite(
                config.GetConnectionString("MarketDb"),
                sqlite => sqlite.MigrationsAssembly("MarketGateway.Data"));
        });
        
        return services;
    }
}
