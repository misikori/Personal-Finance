namespace Portfolio.Core.Services;

/// <summary>
/// Service for converting between currencies
/// </summary>
public interface ICurrencyConverter
{
    /// <summary>
    /// Converts amount from one currency to another
    /// </summary>
    Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount);
}

