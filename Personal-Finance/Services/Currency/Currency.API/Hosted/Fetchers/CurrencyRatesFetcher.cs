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
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(options?.Value?.RatesUrl))
            {
                throw new InvalidOperationException("CurrencyApi:RatesUrl must be configured in appsettings.json");
            }
            _ratesUrl = options?.Value?.RatesUrl ?? throw new ArgumentNullException(nameof(options), "RatesUrl must be configured");
        }

        public async Task<List<CurrencyRate>> FetchRatesAsync()
        {
            _logger.LogInformation($"Fetching rates from {_ratesUrl}.");
            var rates = await _httpClient.GetStringAsync(_ratesUrl);
            var ratesList = JsonConvert.DeserializeObject<CurrencyRateResponseDTO>(rates);
            if (ratesList == null)
            {
                return new List<CurrencyRate>(); //return empty list when fetching is unssuccessful
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
