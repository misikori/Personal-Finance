using System.Globalization;
using MarketGateway.Data;
using MarketGateway.Data.Interfaces;
using MarketGateway.Data.Services;
using MarketGateway.Data.Services.Ohlcv;
using MarketGateway.Data.Services.Quotes;
using MarketGateway.Data.Time;
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
        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var log = loggerFactory.CreateLogger("Storage");

        log.LogInformation("Storage mode: {Mode}", opts.Mode);
        log.LogInformation("Storage DatabasePath: {DbPath}", opts.DatabasePath);
        log.LogInformation("Storage RootDirectory: {Root}", opts.RootDirectory);

        if (string.Equals(opts.Mode, "File", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IStorageService>(_ => new FileStorageService(opts.RootDirectory));
        }
        else
        {
            services.AddScoped<IStorageService, DatabaseStorageService>();
        }

        services.Configure<AppDateOptions>(cfg.GetSection("AppDate"));
        services.AddScoped<IQuoteStorage, QuoteStorage>();
        services.AddScoped<IOhlcvStorage, OhlcvStorage>();

        services.AddSingleton<IAppDate>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<AppDateOptions>>().Value;
            if (string.Equals(opt.Mode, "Fixed", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(opt.FixedDate))
                    throw new InvalidOperationException("AppDate:FixedDate is required when Mode=Fixed.");
                var date = DateOnly.ParseExact(opt.FixedDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                return new FixedAppDate(date);
            }

            return new UtcAppDate();
        });

        return services;
    }
    
}