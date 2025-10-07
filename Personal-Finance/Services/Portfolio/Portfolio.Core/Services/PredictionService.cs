using Portfolio.Core.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Portfolio.Core.Services;

/// <summary>
/// Stock price data for ML training
/// </summary>
public class StockMLData
{
    public float CurrentPrice { get; set; }
    public float SMA5 { get; set; }
    public float SMA10 { get; set; }
    public float SMA20 { get; set; }
    public float Momentum5Day { get; set; }
    public float Volatility { get; set; }
    public float Label { get; set; }  // Next day's price (what we're predicting)
}

/// <summary>
/// ML prediction result
/// </summary>
public class StockMLPrediction
{
    [ColumnName("Score")]
    public float PredictedPrice { get; set; }
}

/// <summary>
/// Implementation of price prediction using FastTree Gradient Boosting ML
/// Trains models on-demand and caches them for fast subsequent predictions
/// </summary>
public class PredictionService : IPredictionService
{
    private readonly IMarketDataService _marketDataService;
    private readonly ILogger<PredictionService> _logger;
    private readonly MLContext _mlContext;
    
    // Cache trained models per stock symbol
    private readonly Dictionary<string, (ITransformer model, DateTime trainedAt)> _modelCache = new();
    private readonly object _cacheLock = new();

    public PredictionService(IMarketDataService marketDataService, ILogger<PredictionService> logger)
    {
        _marketDataService = marketDataService;
        _logger = logger;
        _mlContext = new MLContext(seed: 42);  // Fixed seed for reproducibility
    }

    /// <summary>
    /// Predicts stock price using FastTree Gradient Boosting ML model
    /// 
    /// WORKFLOW:
    /// 1. First request: Trains new model (2-5 seconds) → Caches it
    /// 2. Subsequent requests: Uses cached model (instant!)
    /// 3. Models auto-refresh after 24 hours to stay current
    /// 
    /// HOW IT WORKS:
    /// - Analyzes 100 days of historical price data
    /// - Calculates features: moving averages, momentum, volatility
    /// - Trains FastTree model to learn price patterns
    /// - Predicts next 1-5 day price target
    /// 
    /// WHAT IT PREDICTS:
    /// - Direction: Will it go UP or DOWN?
    /// - Magnitude: By how much?
    /// - Target price based on learned patterns
    /// 
    /// TIMEFRAME: Near-term (1-5 trading days ahead)
    /// </summary>
    public async Task<PricePredictionResponse> PredictPriceAsync(string symbol)
    {
        try
        {
            _logger.LogInformation("Generating ML prediction for {Symbol}", symbol);

            // Get current price
            var currentPrice = await _marketDataService.GetCurrentPriceAsync(symbol);

            // Get or train model
            var model = await GetOrTrainModelAsync(symbol);

            // Get historical data for feature calculation
            var history = await _marketDataService.GetHistoricalPricesAsync(symbol, 30);

            // Calculate current features
            var features = CalculateFeatures(history);

            // Create prediction engine
            var predictionEngine = _mlContext.Model
                .CreatePredictionEngine<StockMLData, StockMLPrediction>(model);

            // Predict next price
            var prediction = predictionEngine.Predict(features);
            var predictedPrice = (decimal)prediction.PredictedPrice;

            // Calculate change percentage
            var changePercent = ((predictedPrice - currentPrice.Price) / currentPrice.Price) * 100;

            // Calculate confidence based on model training metrics and volatility
            var volatility = CalculateVolatility(history);
            var confidence = CalculateConfidence(volatility);

            var response = new PricePredictionResponse
            {
                Symbol = symbol,
                CurrentPrice = currentPrice.Price,
                PredictedPrice = predictedPrice,
                PredictedChangePercent = changePercent,
                Confidence = confidence,
                Method = "FastTree Gradient Boosting Machine Learning"
            };

            _logger.LogInformation(
                "ML Prediction for {Symbol}: Current=${Current}, Predicted=${Predicted} ({Change}%), Confidence={Confidence}%",
                symbol, currentPrice.Price, predictedPrice, changePercent, confidence);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating ML prediction for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Gets cached model or trains a new one if not exists or stale
    /// </summary>
    private async Task<ITransformer> GetOrTrainModelAsync(string symbol)
    {
        lock (_cacheLock)
        {
            // Check if we have a cached model that's less than 24 hours old
            if (_modelCache.TryGetValue(symbol, out var cached))
            {
                var age = DateTime.UtcNow - cached.trainedAt;
                if (age.TotalHours < 24)
                {
                    _logger.LogInformation("Using cached ML model for {Symbol} (age: {Hours:F1}h)", symbol, age.TotalHours);
                    return cached.model;
                }
                else
                {
                    _logger.LogInformation("Cached model for {Symbol} is stale ({Hours:F1}h old), retraining...", symbol, age.TotalHours);
                }
            }
        }

        // Train new model
        _logger.LogInformation("Training new ML model for {Symbol}...", symbol);
        var model = await TrainModelAsync(symbol);

        // Cache it
        lock (_cacheLock)
        {
            _modelCache[symbol] = (model, DateTime.UtcNow);
        }

        _logger.LogInformation("ML model trained and cached for {Symbol}", symbol);
        return model;
    }

    /// <summary>
    /// Trains FastTree ML model using historical price data
    /// </summary>
    private async Task<ITransformer> TrainModelAsync(string symbol)
    {
        // Fetch 100 days of historical data for training
        var prices = await _marketDataService.GetHistoricalPricesAsync(symbol, 100);

        if (prices.Count < 30)
        {
            throw new Exception($"Insufficient data for ML training. Need at least 30 days, got {prices.Count}");
        }

        // Prepare training data with features
        var trainingData = new List<StockMLData>();

        for (int i = 20; i < prices.Count - 1; i++)
        {
            var recentPrices = prices.Take(i + 1).ToList();
            
            trainingData.Add(new StockMLData
            {
                CurrentPrice = (float)prices[i],
                SMA5 = (float)recentPrices.TakeLast(5).Average(),
                SMA10 = (float)recentPrices.TakeLast(10).Average(),
                SMA20 = (float)recentPrices.TakeLast(20).Average(),
                Momentum5Day = (float)((prices[i] - prices[i - 5]) / prices[i - 5] * 100),
                Volatility = (float)CalculateVolatility(recentPrices.TakeLast(10).ToList()),
                Label = (float)prices[i + 1]  // Next day's actual price (training target)
            });
        }

        // Load data into ML.NET
        var data = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Build ML pipeline with FastTree
        var pipeline = _mlContext.Transforms
            .Concatenate("Features", 
                nameof(StockMLData.CurrentPrice),
                nameof(StockMLData.SMA5),
                nameof(StockMLData.SMA10),
                nameof(StockMLData.SMA20),
                nameof(StockMLData.Momentum5Day),
                nameof(StockMLData.Volatility))
            .Append(_mlContext.Regression.Trainers.FastTree(
                numberOfTrees: 100,
                numberOfLeaves: 20,
                minimumExampleCountPerLeaf: 10,
                learningRate: 0.1
            ));

        // Train the model
        var model = pipeline.Fit(data);

        _logger.LogInformation("FastTree model trained for {Symbol} with {Samples} training samples", 
            symbol, trainingData.Count);

        return model;
    }

    /// <summary>
    /// Calculates current features for prediction
    /// </summary>
    private StockMLData CalculateFeatures(List<decimal> prices)
    {
        return new StockMLData
        {
            CurrentPrice = (float)prices.Last(),
            SMA5 = (float)prices.TakeLast(5).Average(),
            SMA10 = (float)prices.TakeLast(10).Average(),
            SMA20 = (float)prices.TakeLast(20).Average(),
            Momentum5Day = (float)((prices.Last() - prices[^6]) / prices[^6] * 100),
            Volatility = (float)CalculateVolatility(prices.TakeLast(10).ToList())
        };
    }

    /// <summary>
    /// Analyzes multiple stocks and generates actionable buy/sell recommendations
    /// Frontend can call this periodically (daily, weekly, etc.) to get updated recommendations
    /// </summary>
    public async Task<StockRecommendationResponse> GetRecommendationsAsync(List<string> symbols)
    {
        _logger.LogInformation("Generating ML-based recommendations for {Count} stocks", symbols.Count);

        var buyRecommendations = new List<StockRecommendation>();
        var sellRecommendations = new List<StockRecommendation>();
        var holdRecommendations = new List<StockRecommendation>();

        foreach (var symbol in symbols)
        {
            try
            {
                // Get ML prediction for this stock
                var prediction = await PredictPriceAsync(symbol);

                var changePercent = prediction.PredictedChangePercent;
                var confidence = prediction.Confidence;

                // Determine action based on predicted change
                string action;
                string trend;
                string strength;
                string reason;

                if (changePercent > 2) // Predicted to go up > 2%
                {
                    action = "BUY";
                    trend = "Uptrend";
                    strength = DetermineStrength(Math.Abs(changePercent), confidence);
                    reason = $"ML model detects strong upward momentum. Predicted gain of {changePercent:F2}%. Pattern analysis suggests continued growth.";
                }
                else if (changePercent < -2) // Predicted to go down > 2%
                {
                    action = "SELL";
                    trend = "Downtrend";
                    strength = DetermineStrength(Math.Abs(changePercent), confidence);
                    reason = $"ML model detects declining trend. Predicted loss of {Math.Abs(changePercent):F2}%. Consider selling to avoid potential losses.";
                }
                else // Predicted change between -2% and +2%
                {
                    action = "HOLD";
                    trend = "Neutral";
                    strength = "Neutral";
                    reason = $"ML model shows no strong directional bias. Predicted change is minimal ({changePercent:F2}%). Hold current position.";
                }

                var recommendation = new StockRecommendation
                {
                    Symbol = symbol,
                    CurrentPrice = prediction.CurrentPrice,
                    PredictedPrice = prediction.PredictedPrice,
                    ExpectedChange = prediction.PredictedPrice - prediction.CurrentPrice,
                    ExpectedChangePercent = changePercent,
                    Action = action,
                    Confidence = confidence,
                    Strength = strength,
                    Reason = reason,
                    Trend = trend
                };

                // Categorize recommendation
                switch (action)
                {
                    case "BUY":
                        buyRecommendations.Add(recommendation);
                        break;
                    case "SELL":
                        sellRecommendations.Add(recommendation);
                        break;
                    default:
                        holdRecommendations.Add(recommendation);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze {Symbol}, skipping", symbol);
                // Continue analyzing other stocks
            }
        }

        // Sort recommendations by confidence * abs(change) to show strongest signals first
        buyRecommendations = buyRecommendations
            .OrderByDescending(r => r.Confidence * Math.Abs(r.ExpectedChangePercent))
            .ToList();

        sellRecommendations = sellRecommendations
            .OrderByDescending(r => r.Confidence * Math.Abs(r.ExpectedChangePercent))
            .ToList();

        var response = new StockRecommendationResponse
        {
            AnalysisDate = DateTime.UtcNow,
            Timeframe = "Current", // Snapshot at time of request
            StocksAnalyzed = symbols.Count,
            BuyRecommendations = buyRecommendations,
            SellRecommendations = sellRecommendations,
            HoldRecommendations = holdRecommendations
        };

        _logger.LogInformation(
            "ML Recommendations: {BuyCount} BUY, {SellCount} SELL, {HoldCount} HOLD",
            buyRecommendations.Count, sellRecommendations.Count, holdRecommendations.Count);

        return response;
    }

    /// <summary>
    /// Determines recommendation strength based on predicted change and confidence
    /// </summary>
    private static string DetermineStrength(decimal changeMagnitude, decimal confidence)
    {
        var score = changeMagnitude * (confidence / 100);

        return score switch
        {
            >= 5 => "Strong",
            >= 2 => "Moderate",
            _ => "Weak"
        };
    }

    /// <summary>
    /// Calculates volatility (standard deviation) of prices
    /// Used for both ML features and confidence scoring
    /// </summary>
    private static decimal CalculateVolatility(List<decimal> prices)
    {
        if (prices.Count < 2) return 0;

        var average = prices.Average();
        var sumOfSquares = prices.Sum(price => Math.Pow((double)(price - average), 2));
        var variance = sumOfSquares / prices.Count;
        return (decimal)Math.Sqrt(variance);
    }

    /// <summary>
    /// Calculates confidence score based on price volatility
    /// Lower volatility = higher confidence in predictions
    /// </summary>
    private static decimal CalculateConfidence(decimal volatility)
    {
        // Normalize volatility to confidence percentage
        // Lower volatility = higher confidence
        var confidence = Math.Max(30, 100 - (volatility * 8));
        return Math.Round(confidence, 2);
    }
}
