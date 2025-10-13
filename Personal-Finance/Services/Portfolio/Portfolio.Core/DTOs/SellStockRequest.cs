namespace Portfolio.Core.DTOs;

/// <summary>
/// Request to sell stock
/// </summary>
public class SellStockRequest
{
    /// <summary>
    /// User ID (extracted from JWT token, not from request body)
    /// Required for: portfolio position tracking, transaction history, and security validation (wallet ownership)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Wallet ID to add proceeds to (must belong to the authenticated user)
    /// </summary>
    public string WalletId { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock symbol to sell (e.g., "AAPL")
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of shares to sell
    /// </summary>
    public decimal Quantity { get; set; }
}


