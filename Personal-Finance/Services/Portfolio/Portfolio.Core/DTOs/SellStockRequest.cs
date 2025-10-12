namespace Portfolio.Core.DTOs;

/// <summary>
/// Request to sell stock
/// </summary>
public class SellStockRequest
{
    /// <summary>
    /// User ID (extracted from JWT token, not from request body)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock symbol to sell (e.g., "AAPL")
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of shares to sell
    /// </summary>
    public decimal Quantity { get; set; }
}


