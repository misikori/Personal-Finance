using Currency.Common.Entities;

namespace Currency.Common.Helpers
{
    public static class CurrencyConverter
    {
        public static decimal Convert(decimal amount, CurrencyRate fromRate, CurrencyRate toRate)
        {
            ArgumentNullException.ThrowIfNull(fromRate);

            return toRate == null
                ? throw new ArgumentNullException(nameof(toRate))
                : amount * (fromRate.ExchangeMiddle / fromRate.Parity) * (toRate.Parity / toRate.ExchangeMiddle);
        }
    }
}
