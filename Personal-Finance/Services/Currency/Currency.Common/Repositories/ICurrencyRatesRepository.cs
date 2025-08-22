using Currency.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Common.Repositories
{
    public interface ICurrencyRatesRepository
    {
        Task<CurrencyRateListDTO> GetRates();
        Task<CurrencyRateListDTO> UpdateRates(CurrencyRateListDTO currencyRateList);
        Task DeleteRates();
    }
}
