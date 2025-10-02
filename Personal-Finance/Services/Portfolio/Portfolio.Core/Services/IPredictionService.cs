using Portfolio.Core.DTOs;

namespace Portfolio.Core.Services;

/// <summary>
/// Service for generating stock price predictions and recommendations
/// </summary>
public interface IPredictionService
{
    /// <summary>
    /// Predicts future stock price based on historical data
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <returns>Price prediction with confidence level</returns>
    Task<PricePredictionResponse> PredictPriceAsync(string symbol);

    /// <summary>
    /// Analyzes multiple stocks and generates buy/sell recommendations
    /// Frontend can call this daily/weekly/monthly as needed
    /// </summary>
    /// <param name="symbols">List of stock symbols to analyze</param>
    /// <returns>Categorized recommendations with confidence levels</returns>
    Task<StockRecommendationResponse> GetRecommendationsAsync(List<string> symbols);
}
