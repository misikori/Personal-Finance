namespace Portfolio.Core.DTOs;

/// <summary>
/// Request to buy stock
/// </summary>
public class BuyStockRequest
{
    /// <summary>
    /// User ID (extracted from JWT token, not from request body)
    /// Required for: portfolio position tracking, transaction history, and security validation (wallet ownership)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Wallet ID to deduct funds from (must belong to the authenticated user)
    /// </summary>
    public string WalletId { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock symbol to purchase (e.g., "AAPL", "TSLA")
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of shares to buy
    /// </summary>
    public decimal Quantity { get; set; }
}


