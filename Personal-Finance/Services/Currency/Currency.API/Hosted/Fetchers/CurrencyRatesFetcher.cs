using Currency.API.Configuration;
using Currency.Common.DTOs;
using Currency.Common.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Currency.API.Hosted.Fetchers
{
    public class CurrencyRatesFetcher : ICurrencyRatesFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyRatesFetcher> _logger;
        private readonly string _ratesUrl;

        // should maybe log if exeption handles when API call is being executed
        public CurrencyRatesFetcher(HttpClient httpClient, IOptions<CurrencyApiSettings> options, ILogger<CurrencyRatesFetcher> logger)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(options?.Value?.RatesUrl))
            {
                throw new InvalidOperationException("CurrencyApi:RatesUrl must be configured in appsettings.json");
            }
            this._ratesUrl = options?.Value?.RatesUrl ?? throw new ArgumentNullException(nameof(options), "RatesUrl must be configured");
        }

        public async Task<List<CurrencyRate>> FetchRatesAsync()
        {
            this._logger.LogInformation($"Fetching rates from {this._ratesUrl}.");
            string rates = await this._httpClient.GetStringAsync(this._ratesUrl);
            CurrencyRateResponseDTO? ratesList = JsonConvert.DeserializeObject<CurrencyRateResponseDTO>(rates);
            if (ratesList == null)
            {
                return []; //return empty list when fetching is unssuccessful
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
