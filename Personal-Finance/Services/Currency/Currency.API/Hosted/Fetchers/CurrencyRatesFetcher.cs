using Currency.Common.DTOs;
using Currency.Common.Entities;
using Newtonsoft.Json;

namespace Currency.API.Hosted.Fetchers
{
    public class CurrencyRatesFetcher : ICurrencyRatesFetcher
    {
        private readonly HttpClient _httpClient;

        // should maybe log if exeption handles when API call is being executed
        public CurrencyRatesFetcher(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<CurrencyRate>> FetchRatesAsync()
        {
            var rates = await _httpClient.GetStringAsync("https://kurs.resenje.org/api/v1/rates/today");
            var ratesList = JsonConvert.DeserializeObject<CurrencyRateResponseDTO>(rates);
            if (ratesList == null)
            {
                return null;
            }

            ratesList.Rates.Insert(0, new CurrencyRate
            {
                Code = "RSD",
                Parity = 1,
                Date = DateOnly.FromDateTime(DateTime.Now),
                ExchangeMiddle = 1
            });
            return ratesList.Rates;
        }
    }
}
