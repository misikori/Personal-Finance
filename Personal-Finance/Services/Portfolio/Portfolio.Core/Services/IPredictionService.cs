using Portfolio.Core.DTOs;

namespace Portfolio.Core.Services;

/// <summary>
/// Service for generating stock price predictions
/// </summary>
public interface IPredictionService
{
    /// <summary>
    /// Predicts future stock price based on historical data
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <returns>Price prediction with confidence level</returns>
    Task<PricePredictionResponse> PredictPriceAsync(string symbol);
}


