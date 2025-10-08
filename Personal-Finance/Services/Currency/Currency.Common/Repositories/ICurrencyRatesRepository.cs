using Currency.Common.DTOs;

namespace Currency.Common.Repositories
{
    public interface ICurrencyRatesRepository
    {
        Task<CurrencyRateListDTO> GetRates();
        Task<CurrencyRateListDTO> UpdateRates(CurrencyRateListDTO currencyRateList);
        Task DeleteRates();
    }
}
