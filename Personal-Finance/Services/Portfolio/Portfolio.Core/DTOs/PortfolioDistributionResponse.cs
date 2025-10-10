namespace Portfolio.Core.DTOs;

/// <summary>
/// Portfolio distribution data for pie chart visualization
/// </summary>
public class PortfolioDistributionResponse
{
    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Base currency used for calculations (e.g., "USD", "EUR")
    /// All values are converted to this currency
    /// </summary>
    public string BaseCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Total portfolio value (converted to base currency)
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Individual stock holdings with percentages
    /// </summary>
    public List<StockDistribution> Holdings { get; set; } = new();
}

/// <summary>
/// Individual stock distribution in portfolio
/// </summary>
public class StockDistribution
{
    /// <summary>
    /// Stock symbol (e.g., AAPL, TSLA)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Number of shares owned
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Current value of this position (converted to base currency)
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Percentage of total portfolio (0-100)
    /// </summary>
    public decimal Percentage { get; set; }
    
    /// <summary>
    /// Original currency of this stock
    /// </summary>
    public string OriginalCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Current price per share
    /// </summary>
    public decimal CurrentPrice { get; set; }
    
    /// <summary>
    /// Currency of this position (e.g., "USD", "EUR", "GBP")
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Suggested color for pie chart (hex color code)
    /// </summary>
    public string Color { get; set; } = string.Empty;
}

