namespace Portfolio.Core.DTOs;

/// <summary>
/// Request to buy stock
/// </summary>
public class BuyStockRequest
{
    /// <summary>
    /// User ID (extracted from JWT token, not from request body)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock symbol to purchase (e.g., "AAPL", "TSLA")
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of shares to buy
    /// </summary>
    public decimal Quantity { get; set; }
}


