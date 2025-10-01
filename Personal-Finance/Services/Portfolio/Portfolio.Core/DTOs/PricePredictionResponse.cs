namespace Portfolio.Core.DTOs;

/// <summary>
/// Price prediction for a stock based on historical data
/// </summary>
public class PricePredictionResponse
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
    /// Predicted price for the next period
    /// </summary>
    public decimal PredictedPrice { get; set; }
    
    /// <summary>
    /// Expected change in percentage
    /// </summary>
    public decimal PredictedChangePercent { get; set; }
    
    /// <summary>
    /// Confidence level of the prediction (0-100)
    /// </summary>
    public decimal Confidence { get; set; }
    
    /// <summary>
    /// Prediction method used (e.g., "Simple Moving Average")
    /// </summary>
    public string Method { get; set; } = string.Empty;
    
    /// <summary>
    /// When this prediction was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}


