using Currency.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Common.Helpers
{
    public static class CurrencyConverter
    {
        public static decimal Convert(decimal amount, CurrencyRate fromRate, CurrencyRate toRate)
        {
            if (fromRate == null) throw new ArgumentNullException(nameof(fromRate));
            if (toRate == null) throw new ArgumentNullException(nameof(toRate));

            decimal rsdValue = amount * (fromRate.ExchangeMiddle / fromRate.Parity);
            decimal converted = rsdValue * (toRate.Parity / toRate.ExchangeMiddle);

            return converted;
        }
    }
}
