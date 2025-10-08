namespace Portfolio.Core.DTOs;

/// <summary>
/// ML-based price prediction for a stock
/// Uses FastTree Gradient Boosting machine learning algorithm
/// Predicts near-term price movement (1-5 days ahead)
/// </summary>
public class PricePredictionResponse
{
    /// <summary>
    /// Stock symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Current market price (as of now)
    /// </summary>
    public decimal CurrentPrice { get; set; }
    
    /// <summary>
    /// ML predicted price target
    /// This represents where the price is LIKELY TO MOVE in the next 1-5 trading days
    /// Based on learned patterns from 100 days of historical data
    /// </summary>
    public decimal PredictedPrice { get; set; }
    
    /// <summary>
    /// Expected change in percentage
    /// Example: +3.5% means ML model predicts stock will go UP by 3.5%
    ///          -2.1% means ML model predicts stock will go DOWN by 2.1%
    /// </summary>
    public decimal PredictedChangePercent { get; set; }
    
    /// <summary>
    /// ML model confidence level (0-100%)
    /// Based on historical prediction accuracy and price volatility
    /// 
    /// 80-100% = High confidence (stock is stable, pattern is clear)
    /// 50-79%  = Medium confidence (some volatility)
    /// 0-49%   = Low confidence (highly volatile, prediction uncertain)
    /// </summary>
    public decimal Confidence { get; set; }
    
    /// <summary>
    /// Machine learning algorithm used
    /// "FastTree Gradient Boosting Machine Learning"
    /// </summary>
    public string Method { get; set; } = string.Empty;
    
    /// <summary>
    /// When this prediction was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
