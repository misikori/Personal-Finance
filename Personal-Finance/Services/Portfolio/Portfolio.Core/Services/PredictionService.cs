using Portfolio.Core.DTOs;
using Microsoft.Extensions.Logging;

namespace Portfolio.Core.Services;

/// <summary>
/// Implementation of price prediction using simple moving averages and trend analysis
/// </summary>
public class PredictionService : IPredictionService
{
    private readonly IMarketDataService _marketDataService;
    private readonly ILogger<PredictionService> _logger;

    public PredictionService(IMarketDataService marketDataService, ILogger<PredictionService> logger)
    {
        _marketDataService = marketDataService;
        _logger = logger;
    }

    /// <summary>
    /// Predicts next day's price using Simple Moving Average (SMA) and trend analysis
    /// This is a basic implementation - in production, you'd use machine learning models
    /// </summary>
    /// <param name="symbol">Stock symbol to predict</param>
    /// <returns>Prediction with confidence level based on historical volatility</returns>
    public async Task<PricePredictionResponse> PredictPriceAsync(string symbol)
    {
        try
        {
            _logger.LogInformation("Generating price prediction for {Symbol}", symbol);
            
            // Get 30 days of historical data
            var historicalPrices = await _marketDataService.GetHistoricalPricesAsync(symbol, 30);
            
            if (historicalPrices.Count < 10)
            {
                throw new Exception($"Insufficient historical data for {symbol}. Need at least 10 days.");
            }

            var currentPrice = await _marketDataService.GetCurrentPriceAsync(symbol);

            // Calculate Simple Moving Average (SMA) for different periods
            var sma5 = CalculateSMA(historicalPrices.TakeLast(5).ToList());
            var sma10 = CalculateSMA(historicalPrices.TakeLast(10).ToList());
            var sma20 = CalculateSMA(historicalPrices.TakeLast(20).ToList());

            // Calculate trend: if short-term SMA > long-term SMA = uptrend
            var trend = (sma5 - sma20) / sma20;

            // Predict next price based on trend
            var predictedPrice = currentPrice.Price + (currentPrice.Price * trend);

            // Calculate volatility (standard deviation) for confidence
            var volatility = CalculateStandardDeviation(historicalPrices);
            
            // Confidence decreases with higher volatility
            var confidence = Math.Max(20, 100 - (volatility * 10));

            var prediction = new PricePredictionResponse
            {
                Symbol = symbol,
                CurrentPrice = currentPrice.Price,
                PredictedPrice = predictedPrice,
                PredictedChangePercent = ((predictedPrice - currentPrice.Price) / currentPrice.Price) * 100,
                Confidence = Math.Round(confidence, 2),
                Method = "Simple Moving Average (SMA) with Trend Analysis"
            };

            _logger.LogInformation(
                "Prediction for {Symbol}: Current=${Current}, Predicted=${Predicted} ({Change}%), Confidence={Confidence}%",
                symbol, currentPrice.Price, predictedPrice, prediction.PredictedChangePercent, confidence);

            return prediction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prediction for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Calculates Simple Moving Average from a list of prices
    /// </summary>
    private static decimal CalculateSMA(List<decimal> prices)
    {
        return prices.Average();
    }

    /// <summary>
    /// Calculates standard deviation to measure price volatility
    /// Higher volatility = lower prediction confidence
    /// </summary>
    private static decimal CalculateStandardDeviation(List<decimal> prices)
    {
        var average = prices.Average();
        var sumOfSquares = prices.Sum(price => Math.Pow((double)(price - average), 2));
        var variance = sumOfSquares / prices.Count;
        return (decimal)Math.Sqrt(variance);
    }
}

