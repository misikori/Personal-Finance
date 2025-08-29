using Currency.API.Hosted.Fetchers;
using Currency.Common.DTOs;
using Currency.Common.Entities;
using Currency.Common.Repositories;

namespace Currency.API.Hosted
{
    public class CurrencyHostedService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICurrencyRatesFetcher _fetcher;
        private readonly ILogger<CurrencyHostedService> _logger;

        private Timer? _timer = null;

        public CurrencyHostedService(IServiceScopeFactory scopeFactory, ICurrencyRatesFetcher fetcher, ILogger<CurrencyHostedService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Currency hosted service for fething rates running");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
        }

        private async void DoWork(object? state)
        {
            _logger.LogInformation("Fetching rates from https://kurs.resenje.org/api/v1/rates/today.");
            var rates = await _fetcher.FetchRatesAsync();

            if (rates == null)
            {
                _logger.LogError("Fetching rates from https://kurs.resenje.org/api/v1/rates/today unseccessful.");
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<ICurrencyRatesRepository>();
                await repository.UpdateRates(new CurrencyRateListDTO { Key = "rates:global", Rates = rates });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            _logger.LogInformation("Currency hosted service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);
            
            return Task.CompletedTask;   
        }
    }
}
