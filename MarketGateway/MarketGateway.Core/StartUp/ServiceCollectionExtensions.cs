using System.Net;
using MarketGateway.Application;
using MarketGateway.Data;
using MarketGateway.Data.Interfaces;
using MarketGateway.Interfaces;
using MarketGateway.Mapping;
using MarketGateway.Providers.Configuration;
using MarketGateway.Providers.Interfaces;
using MarketGateway.Providers.Parsing;
using MarketGateway.Providers.Providers;
using MarketGateway.Services;
using Microsoft.EntityFrameworkCore;

namespace MarketGateway.StartUp;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarketGateway(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<MarketDbContext>(options =>
            options.UseSqlite(config.GetConnectionString("MarketDb"),
                sqlite => sqlite.MigrationsAssembly("MarketGateway.Data")));
        
        services.AddSingleton<IVendorResponseParser, VendorResponseParser>();
        services.AddSingleton<IStorageService>(new FileStorageService("DataStorage"));
        services.AddSingleton(TimeProvider.System);
        
        services.AddScoped<IApiUsageTracker, ApiUsageTracker>();
        services.AddAutoMapper(typeof(ProtoMappingProfile).Assembly);


        var vendorsPath = config["Vendors:Folder"]
                          ?? "../MarketGateway.Providers/Configuration/Vendors";
        var vendorConfigs = VendorConfigLoader.LoadFromFolder(vendorsPath);
        services.AddSingleton<IReadOnlyList<VendorConfig>>(vendorConfigs);
        
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
                var http = httpFactory;
                var usageTracker = sp.GetRequiredService<IApiUsageTracker>();
                var parser      = sp.GetRequiredService<IVendorResponseParser>();
                var logger = sp.GetRequiredService<ILogger<VendorMarketDataProvider>>();

                return new VendorMarketDataProvider(conf, http, usageTracker,parser, logger);
            });
        }
        
        services.AddScoped<IMarketDataProviderResolver, MarketDataProviderResolver>();
        services.AddScoped<IMarketDataBroker, MarketDataBroker>();
        
        return services;
    }
}
