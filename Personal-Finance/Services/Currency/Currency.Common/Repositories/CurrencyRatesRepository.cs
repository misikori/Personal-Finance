using Currency.Common.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Currency.Common.Repositories
{
    public class CurrencyRatesRepository(IDistributedCache cache) : ICurrencyRatesRepository
    {
        private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));

        public async Task<CurrencyRateListDTO> GetRates()
        {
            string? rates = await this._cache.GetStringAsync("rates:global");

            return string.IsNullOrEmpty(rates)
                ? new CurrencyRateListDTO { Rates = [] }
                : JsonConvert.DeserializeObject<CurrencyRateListDTO>(rates) ?? new CurrencyRateListDTO { Rates = [] };
        }

        public async Task<CurrencyRateListDTO> UpdateRates(CurrencyRateListDTO currencyRateList)
        {
            string ratesString = JsonConvert.SerializeObject(currencyRateList);

            await this._cache.SetStringAsync("rates:global", ratesString);

            return await this.GetRates();
        }

        public async Task DeleteRates() => await this._cache.RemoveAsync("rates:global");
    }
}
