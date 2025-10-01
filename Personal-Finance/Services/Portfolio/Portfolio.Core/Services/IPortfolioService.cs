using Portfolio.Core.DTOs;
using Portfolio.Core.Entities;

namespace Portfolio.Core.Services;

/// <summary>
/// Main portfolio management service
/// </summary>
public interface IPortfolioService
{
    /// <summary>
    /// Executes a buy order for stocks
    /// </summary>
    Task<Transaction> BuyStockAsync(BuyStockRequest request);
    
    /// <summary>
    /// Executes a sell order for stocks
    /// </summary>
    Task<Transaction> SellStockAsync(SellStockRequest request);
    
    /// <summary>
    /// Gets complete portfolio summary with current values and gains/losses
    /// </summary>
    Task<PortfolioSummaryResponse> GetPortfolioSummaryAsync(string username);
    
    /// <summary>
    /// Checks if user has sufficient budget to buy stocks
    /// NOTE: This will integrate with Budget service when available
    /// </summary>
    Task<bool> CheckBudgetAsync(string username, decimal amount);
}


