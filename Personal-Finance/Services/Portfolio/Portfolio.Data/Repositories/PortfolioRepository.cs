using Microsoft.EntityFrameworkCore;
using Portfolio.Core.Entities;
using Portfolio.Core.Repositories;

namespace Portfolio.Data.Repositories;

/// <summary>
/// SQL Server implementation of portfolio repository using Entity Framework Core
/// </summary>
public class PortfolioRepository : IPortfolioRepository
{
    private readonly PortfolioDbContext _context;

    public PortfolioRepository(PortfolioDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all positions for a user from the database
    /// </summary>
    public async Task<List<PortfolioPosition>> GetUserPositionsAsync(string username)
    {
        return await _context.Positions
            .Where(p => p.Username == username)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific position by username and symbol
    /// </summary>
    public async Task<PortfolioPosition?> GetPositionAsync(string username, string symbol)
    {
        return await _context.Positions
            .FirstOrDefaultAsync(p => p.Username == username && p.Symbol == symbol);
    }

    /// <summary>
    /// Creates or updates a position in the database
    /// </summary>
    public async Task<PortfolioPosition> UpsertPositionAsync(PortfolioPosition position)
    {
        var existing = await _context.Positions
            .FirstOrDefaultAsync(p => p.Username == position.Username && p.Symbol == position.Symbol);

        if (existing != null)
        {
            // Update existing position
            existing.Quantity = position.Quantity;
            existing.AveragePurchasePrice = position.AveragePurchasePrice;
            existing.LastUpdated = DateTime.UtcNow;
            _context.Positions.Update(existing);
        }
        else
        {
            // Add new position
            position.LastUpdated = DateTime.UtcNow;
            await _context.Positions.AddAsync(position);
        }

        await _context.SaveChangesAsync();
        return existing ?? position;
    }

    /// <summary>
    /// Deletes a position when user sells all shares
    /// </summary>
    public async Task DeletePositionAsync(string username, string symbol)
    {
        var position = await _context.Positions
            .FirstOrDefaultAsync(p => p.Username == username && p.Symbol == symbol);

        if (position != null)
        {
            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Records a transaction in the database
    /// </summary>
    public async Task<Transaction> AddTransactionAsync(Transaction transaction)
    {
        transaction.TransactionDate = DateTime.UtcNow;
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    /// <summary>
    /// Gets transaction history for a user
    /// </summary>
    public async Task<List<Transaction>> GetUserTransactionsAsync(string username)
    {
        return await _context.Transactions
            .Where(t => t.Username == username)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }
}


