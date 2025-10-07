using Currency.API.Hosted.Fetchers;
using Currency.Common.DTOs;
using Currency.Common.Entities;
using Currency.Common.Repositories;

namespace Currency.API.Hosted
{
    public class CurrencyHostedService(IServiceScopeFactory scopeFactory, ICurrencyRatesFetcher fetcher, ILogger<CurrencyHostedService> logger) : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        private readonly ICurrencyRatesFetcher _fetcher = fetcher ?? throw new ArgumentNullException(nameof(fetcher));
        private readonly ILogger<CurrencyHostedService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private Timer? _timer;

        public void Dispose() => this._timer?.Dispose();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Currency hosted service for fething rates running");

            this._timer = new Timer(this.DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            List<CurrencyRate> rates = await this._fetcher.FetchRatesAsync();

            if (rates.Count == 0)
            {
                this._logger.LogError("Fetching rates unseccessful.");
                return;
            }

            using IServiceScope scope = this._scopeFactory.CreateScope();
            ICurrencyRatesRepository repository = scope.ServiceProvider.GetRequiredService<ICurrencyRatesRepository>();
            _ = await repository.UpdateRates(new CurrencyRateListDTO { Key = "rates:global", Rates = rates });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Currency hosted service is stopping.");

            _ = (this._timer?.Change(Timeout.Infinite, 0));

            return Task.CompletedTask;
        }
    }
}
