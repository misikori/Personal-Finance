using Portfolio.Core.Entities;

namespace Portfolio.Core.Repositories;

/// <summary>
/// Repository for managing portfolio positions and transactions
/// </summary>
public interface IPortfolioRepository
{
    /// <summary>
    /// Gets all positions for a specific user
    /// </summary>
    Task<List<PortfolioPosition>> GetUserPositionsAsync(string username);
    
    /// <summary>
    /// Gets a specific position by username and symbol
    /// </summary>
    Task<PortfolioPosition?> GetPositionAsync(string username, string symbol);
    
    /// <summary>
    /// Adds a new position or updates an existing one
    /// </summary>
    Task<PortfolioPosition> UpsertPositionAsync(PortfolioPosition position);
    
    /// <summary>
    /// Removes a position (when quantity reaches zero)
    /// </summary>
    Task DeletePositionAsync(string username, string symbol);
    
    /// <summary>
    /// Adds a transaction record
    /// </summary>
    Task<Transaction> AddTransactionAsync(Transaction transaction);
    
    /// <summary>
    /// Gets transaction history for a user
    /// </summary>
    Task<List<Transaction>> GetUserTransactionsAsync(string username);
}


