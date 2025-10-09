namespace Portfolio.Core.Entities;

/// <summary>
/// Represents a buy or sell transaction
/// </summary>
public class Transaction
{
    /// <summary>
    /// Unique transaction identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Username who executed the transaction
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock symbol (e.g., "AAPL")
    /// </summary>
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of transaction: "BUY" or "SELL"
    /// </summary>
    public string Type { get; set; } = string.Empty; // BUY or SELL
    
    /// <summary>
    /// Number of shares transacted
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// Price per share at the time of transaction
    /// </summary>
    public decimal PricePerShare { get; set; }
    
    /// <summary>
    /// Currency of the transaction (e.g., "USD", "EUR", "GBP")
    /// </summary>
    public required string Currency { get; set; }
    
    /// <summary>
    /// Total transaction value (Quantity * PricePerShare)
    /// </summary>
    public decimal TotalValue => Quantity * PricePerShare;
    
    /// <summary>
    /// When the transaction was executed
    /// </summary>
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
}


