using Currency.Common.Entities;

namespace Currency.API.Hosted.Fetchers
{
    public interface ICurrencyRatesFetcher
    {
        Task<List<CurrencyRate>> FetchRatesAsync();
    }
}
