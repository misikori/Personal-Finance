using Currency.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Common.Repositories
{
    internal interface ICurrencyRatesRepository
    {
        Task<CurrencyRateList> GetRates(string username);
        Task<CurrencyRateList> UpdateRates(CurrencyRateList currencyRateList);

        Task DeleteRates(string username);
    }
}
