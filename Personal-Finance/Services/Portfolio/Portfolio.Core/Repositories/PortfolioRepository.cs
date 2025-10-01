using Portfolio.Core.Entities;

namespace Portfolio.Core.Repositories;

/// <summary>
/// In-memory implementation of portfolio repository
/// NOTE: In production, this would use a database (Entity Framework, Dapper, etc.)
/// </summary>
public class PortfolioRepository : IPortfolioRepository
{
    // In-memory storage (for demo purposes - use a database in production!)
    private readonly List<PortfolioPosition> _positions = new();
    private readonly List<Transaction> _transactions = new();
    private readonly object _lock = new();

    /// <summary>
    /// Retrieves all portfolio positions for a given username
    /// </summary>
    public Task<List<PortfolioPosition>> GetUserPositionsAsync(string username)
    {
        lock (_lock)
        {
            var positions = _positions
                .Where(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Task.FromResult(positions);
        }
    }

    /// <summary>
    /// Gets a specific position by username and stock symbol
    /// </summary>
    public Task<PortfolioPosition?> GetPositionAsync(string username, string symbol)
    {
        lock (_lock)
        {
            var position = _positions
                .FirstOrDefault(p => 
                    p.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    p.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(position);
        }
    }

    /// <summary>
    /// Creates a new position or updates existing one
    /// Used when buying stocks to update average price and quantity
    /// </summary>
    public Task<PortfolioPosition> UpsertPositionAsync(PortfolioPosition position)
    {
        lock (_lock)
        {
            var existing = _positions
                .FirstOrDefault(p => 
                    p.Username.Equals(position.Username, StringComparison.OrdinalIgnoreCase) &&
                    p.Symbol.Equals(position.Symbol, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                _positions.Remove(existing);
            }

            _positions.Add(position);
            return Task.FromResult(position);
        }
    }

    /// <summary>
    /// Deletes a position when user sells all shares
    /// </summary>
    public Task DeletePositionAsync(string username, string symbol)
    {
        lock (_lock)
        {
            var position = _positions
                .FirstOrDefault(p => 
                    p.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    p.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

            if (position != null)
            {
                _positions.Remove(position);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Records a buy or sell transaction for audit trail
    /// </summary>
    public Task<Transaction> AddTransactionAsync(Transaction transaction)
    {
        lock (_lock)
        {
            _transactions.Add(transaction);
            return Task.FromResult(transaction);
        }
    }

    /// <summary>
    /// Gets complete transaction history for a user
    /// </summary>
    public Task<List<Transaction>> GetUserTransactionsAsync(string username)
    {
        lock (_lock)
        {
            var transactions = _transactions
                .Where(t => t.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.TransactionDate)
                .ToList();
            return Task.FromResult(transactions);
        }
    }
}


