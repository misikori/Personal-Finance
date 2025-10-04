using Currency.Common.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Currency.Common.Extensions
{
    public static class CurrencyCommonExtensions
    {
        // we will call it in out projects as service.AddCurrencyCommonServices(_configuration);
        public static void AddCurrencyCommonServices(this IServiceCollection services, IConfiguration configuration)
        {
            _ = services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetValue<string>("CacheSettings:ConnectionString");
            });
            _ = services.AddScoped<ICurrencyRatesRepository, CurrencyRatesRepository>();
        }
    }
}
