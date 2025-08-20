using Currency.Common.Entities;
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
    internal class CurrencyRatesRepository : ICurrencyRatesRepository
    {
        private readonly IDistributedCache _cache;

        public CurrencyRatesRepository(IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<CurrencyRateList> GetRates(string username)
        {
            var rates = await _cache.GetStringAsync(username);

            if (string.IsNullOrEmpty(rates)) {
                return null;
            }

            return JsonConvert.DeserializeObject<CurrencyRateList>(rates);
        }

        public async Task<CurrencyRateList> UpdateRates(CurrencyRateList currencyRateList)
        {
            var ratesString = JsonConvert.SerializeObject(currencyRateList);

            await _cache.SetStringAsync(currencyRateList.Username, ratesString);

            return await GetRates(currencyRateList.Username);
        }

        public async Task DeleteRates(string username)
        {
            await _cache.RemoveAsync(username);
        }
    }
}
