using Currency.API.Hosted.Fetchers;
using Currency.Common.Entities;
using Currency.Common.Repositories;

namespace Currency.API.Hosted
{
    public class CurrencyStartupService : IHostedService
    {
        private readonly ICurrencyRatesRepository _repository;
        private readonly ICurrencyRatesFetcher _fetcher;

        public CurrencyStartupService(ICurrencyRatesRepository repository, ICurrencyRatesFetcher fetcher)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var rates = await _fetcher.FetchRatesAsync();
            await _repository.UpdateRates(new CurrencyRateList { Username = "TEST", Rates = rates});
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
