namespace Portfolio.Core.DTOs;

/// <summary>
/// Complete portfolio summary for a user
/// </summary>
public class PortfolioSummaryResponse
{
    /// <summary>
    /// Username of the portfolio owner
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Total amount invested across all positions
    /// </summary>
    public decimal TotalInvested { get; set; }
    
    /// <summary>
    /// Current market value of the entire portfolio
    /// </summary>
    public decimal CurrentValue { get; set; }
    
    /// <summary>
    /// Total gain/loss in dollar amount
    /// </summary>
    public decimal TotalGainLoss { get; set; }
    
    /// <summary>
    /// Total gain/loss as a percentage
    /// </summary>
    public decimal GainLossPercentage { get; set; }
    
    /// <summary>
    /// List of individual stock positions
    /// </summary>
    public List<PositionDetail> Positions { get; set; } = new();
}

/// <summary>
/// Details of a single stock position in the portfolio
/// </summary>
public class PositionDetail
{
    /// <summary>
    /// Stock symbol
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of shares owned
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Average purchase price per share
    /// </summary>
    public decimal AveragePurchasePrice { get; set; }
    
    /// <summary>
    /// Current market price per share
    /// </summary>
    public decimal CurrentPrice { get; set; }
    
    /// <summary>
    /// Total amount invested in this position
    /// </summary>
    public decimal TotalInvested { get; set; }
    
    /// <summary>
    /// Current market value of this position
    /// </summary>
    public decimal CurrentValue { get; set; }
    
    /// <summary>
    /// Gain or loss in dollar amount for this position
    /// </summary>
    public decimal GainLoss { get; set; }
    
    /// <summary>
    /// Gain or loss as a percentage for this position
    /// </summary>
    public decimal GainLossPercentage { get; set; }
    
    /// <summary>
    /// Date when this position was first opened
    /// </summary>
    public DateTime FirstPurchaseDate { get; set; }
}


