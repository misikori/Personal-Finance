namespace Portfolio.Core.DTOs;

/// <summary>
/// Current stock price information
/// </summary>
public class StockPriceResponse
{
    /// <summary>
    /// Stock symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Current market price
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Opening price of the day
    /// </summary>
    public decimal Open { get; set; }
    
    /// <summary>
    /// Highest price of the day
    /// </summary>
    public decimal High { get; set; }
    
    /// <summary>
    /// Lowest price of the day
    /// </summary>
    public decimal Low { get; set; }
    
    /// <summary>
    /// Previous day's closing price
    /// </summary>
    public decimal PreviousClose { get; set; }
    
    /// <summary>
    /// Trading volume
    /// </summary>
    public decimal Volume { get; set; }
    
    /// <summary>
    /// When this price was fetched
    /// </summary>
    public DateTime AsOf { get; set; }
}


