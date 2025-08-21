using Currency.Common.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Common.Extensions
{
    public static class CurrencyCommonExtensions
    {
        // we will call it in out projects as service.AddCurrencyCommonServices(_configuration);
        public static void AddCurrencyCommonServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options => {
                options.Configuration = configuration.GetValue<string>("CacheSettings:ConnectionString");
            });
            services.AddSingleton<ICurrencyRatesRepository,  CurrencyRatesRepository>();
        }
    }
}
