namespace Portfolio.Core.Entities;

/// <summary>
/// Represents a stock position in a user's portfolio
/// </summary>
public class PortfolioPosition
{
    /// <summary>
    /// Unique identifier for this position
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Username of the portfolio owner
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock symbol (e.g., "AAPL", "TSLA")
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
    /// Total amount invested (Quantity * AveragePurchasePrice)
    /// </summary>
    public decimal TotalInvested => Quantity * AveragePurchasePrice;
    
    /// <summary>
    /// Date when the position was first opened
    /// </summary>
    public DateTime FirstPurchaseDate { get; set; }
    
    /// <summary>
    /// Date of the last transaction for this position
    /// </summary>
    public DateTime LastUpdated { get; set; }
}


