namespace Portfolio.Core.DTOs;

/// <summary>
/// Request to sell stock
/// </summary>
public class SellStockRequest
{
    /// <summary>
    /// Username of the seller
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock symbol to sell (e.g., "AAPL")
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of shares to sell
    /// </summary>
    public decimal Quantity { get; set; }
}


