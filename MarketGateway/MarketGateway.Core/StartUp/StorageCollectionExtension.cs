using MarketGateway.Data;
using MarketGateway.Data.Interfaces;
using MarketGateway.Data.Services;
using MarketGateway.Interfaces;
using Microsoft.Extensions.Options;

namespace MarketGateway.StartUp;

public static class StorageCollectionExtension
{
    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration cfg)
    {
        var section = cfg.GetSection("Storage");
        services.Configure<StorageOptions>(section);
        var opts = section.Get<StorageOptions>() ?? new();

        if (string.Equals(opts.Mode, "File", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IStorageService>(_ => new FileStorageService(opts.RootDirectory));
        }
        else
        {
            services.AddScoped<IStorageService, DatabaseStorageService>();
        }
        
        return services;
    }
    
}