namespace Portfolio.Core.DTOs;

/// <summary>
/// ML-based stock recommendation analysis with buy/sell suggestions
/// Uses FastTree Gradient Boosting to analyze multiple stocks
/// </summary>
public class StockRecommendationResponse
{
    /// <summary>
    /// When the ML analysis was performed
    /// </summary>
    public DateTime AnalysisDate { get; set; }

    /// <summary>
    /// Timeframe used for analysis
    /// "Current" means snapshot-in-time ML predictions
    /// </summary>
    public string Timeframe { get; set; } = string.Empty;

    /// <summary>
    /// Total number of stocks analyzed by ML model
    /// </summary>
    public int StocksAnalyzed { get; set; }

    /// <summary>
    /// List of buy recommendations (stocks ML predicts will go UP)
    /// Sorted by strongest signals first
    /// </summary>
    public List<StockRecommendation> BuyRecommendations { get; set; } = new();

    /// <summary>
    /// List of sell recommendations (stocks ML predicts will go DOWN)
    /// Sorted by strongest signals first
    /// </summary>
    public List<StockRecommendation> SellRecommendations { get; set; } = new();

    /// <summary>
    /// Stocks with no clear ML trend (hold)
    /// </summary>
    public List<StockRecommendation> HoldRecommendations { get; set; } = new();
}

/// <summary>
/// Individual stock recommendation from ML analysis
/// </summary>
public class StockRecommendation
{
    /// <summary>
    /// Stock symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Current price
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// ML predicted price target
    /// </summary>
    public decimal PredictedPrice { get; set; }

    /// <summary>
    /// Expected change amount in dollars
    /// </summary>
    public decimal ExpectedChange { get; set; }

    /// <summary>
    /// Expected change percentage
    /// </summary>
    public decimal ExpectedChangePercent { get; set; }

    /// <summary>
    /// ML recommendation: "BUY", "SELL", or "HOLD"
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// ML model confidence level (0-100%)
    /// Higher = more confident in the prediction
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// Strength of recommendation: "Strong", "Moderate", "Weak"
    /// Based on combination of predicted change and confidence
    /// </summary>
    public string Strength { get; set; } = string.Empty;

    /// <summary>
    /// ML model reasoning behind the recommendation
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Trend direction detected by ML: "Uptrend", "Downtrend", "Neutral"
    /// </summary>
    public string Trend { get; set; } = string.Empty;
}
