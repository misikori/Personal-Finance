using Currency.API.Hosted.Fetchers;
using Currency.Common.DTOs;
using Currency.Common.Entities;
using Currency.Common.Repositories;

namespace Currency.API.Hosted
{
    public class CurrencyStartupService : IHostedService
    {


        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICurrencyRatesFetcher _fetcher;

        public CurrencyStartupService(IServiceScopeFactory scopeFactory, ICurrencyRatesFetcher fetcher)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var rates = await _fetcher.FetchRatesAsync();

            using (var scope = _scopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<ICurrencyRatesRepository>();
                await repository.UpdateRates(new CurrencyRateListDTO { Key = "rates:global", Rates = rates });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
