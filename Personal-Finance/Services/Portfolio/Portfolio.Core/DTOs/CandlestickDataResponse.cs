namespace Portfolio.Core.DTOs;

/// <summary>
/// OHLCV data for candlestick chart visualization
/// </summary>
public class CandlestickDataResponse
{
    /// <summary>
    /// Stock symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Start date of the data range
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the data range
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Number of candlesticks returned
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Candlestick data points (OHLCV)
    /// </summary>
    public List<Candlestick> Data { get; set; } = new();
}

/// <summary>
/// Individual candlestick (OHLCV for one time period)
/// </summary>
public class Candlestick
{
    /// <summary>
    /// Date/time of this candlestick
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Opening price
    /// </summary>
    public decimal Open { get; set; }

    /// <summary>
    /// Highest price during period
    /// </summary>
    public decimal High { get; set; }

    /// <summary>
    /// Lowest price during period
    /// </summary>
    public decimal Low { get; set; }

    /// <summary>
    /// Closing price
    /// </summary>
    public decimal Close { get; set; }

    /// <summary>
    /// Trading volume
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    /// Price change from open to close
    /// </summary>
    public decimal Change { get; set; }

    /// <summary>
    /// Percentage change from open to close
    /// </summary>
    public decimal ChangePercent { get; set; }
}

