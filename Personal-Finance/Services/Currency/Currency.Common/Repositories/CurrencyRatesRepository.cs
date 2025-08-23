using Currency.Common.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Currency.Common.Repositories
{
    public class CurrencyRatesRepository : ICurrencyRatesRepository
    {
        private readonly IDistributedCache _cache;

        public CurrencyRatesRepository(IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<CurrencyRateListDTO> GetRates()
        {
            var rates = await _cache.GetStringAsync("rates:global");

            if (string.IsNullOrEmpty(rates)) {
                return null;
            }

            return JsonConvert.DeserializeObject<CurrencyRateListDTO>(rates);
        }

        public async Task<CurrencyRateListDTO> UpdateRates(CurrencyRateListDTO currencyRateList)
        {
            var ratesString = JsonConvert.SerializeObject(currencyRateList);

            await _cache.SetStringAsync("rates:global", ratesString);

            return await GetRates();
        }

        public async Task DeleteRates()
        {
            await _cache.RemoveAsync("rates:global");
        }
    }
}
