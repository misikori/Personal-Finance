using Portfolio.Core.DTOs;

namespace Portfolio.Core.Services;

/// <summary>
/// Service for fetching market data from MarketGateway via gRPC
/// </summary>
public interface IMarketDataService
{
    /// <summary>
    /// Gets the current price for a specific stock symbol
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., "AAPL")</param>
    /// <returns>Current stock price information</returns>
    Task<StockPriceResponse> GetCurrentPriceAsync(string symbol);
    
    /// <summary>
    /// Gets historical price data for predictions
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <param name="days">Number of days of historical data</param>
    /// <returns>List of historical prices</returns>
    Task<List<decimal>> GetHistoricalPricesAsync(string symbol, int days);

    /// <summary>
    /// Gets full OHLCV candlestick data for charting
    /// Perfect for rendering candlestick charts in the frontend
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <param name="days">Number of days of data to fetch</param>
    /// <returns>Complete OHLCV data for candlestick visualization</returns>
    Task<CandlestickDataResponse> GetCandlestickDataAsync(string symbol, int days);
}
