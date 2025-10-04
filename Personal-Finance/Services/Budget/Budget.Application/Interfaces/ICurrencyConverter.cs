namespace Budget.Application.Interfaces
{
    public interface ICurrencyConverter
    {
        Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount);
    }
}
