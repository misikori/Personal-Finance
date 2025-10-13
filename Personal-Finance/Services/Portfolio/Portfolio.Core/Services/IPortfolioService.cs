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
    /// All values are converted to the specified base currency
    /// </summary>
    Task<PortfolioSummaryResponse> GetPortfolioSummaryAsync(string username, string baseCurrency = "USD");
    
    /// <summary>
    /// Gets portfolio distribution for pie chart visualization
    /// Returns percentage breakdown of holdings by current value
    /// All values are converted to the specified base currency
    /// </summary>
    Task<PortfolioDistributionResponse> GetPortfolioDistributionAsync(string username, string baseCurrency = "USD");
    
    /// <summary>
    /// Checks if wallet has sufficient funds to buy stocks
    /// </summary>
    Task<bool> CheckBudgetAsync(string walletId, decimal amount);
}
