using Budget.Application.Interfaces;
using Budget.Domain.Entities;
using Budget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Budget.Infrastructure.Persistence.Repositories;

public class SpendingLimitRepository : ISpendingLimitRepository
{
    private readonly BudgetDbContext _context;

    public SpendingLimitRepository(BudgetDbContext context)
    {
        _context = context;
    }

    public async Task<SpendingLimit?> GetLimitForMonthAsync(Guid walletId, string categoryName, int month, int year)
    {
        return await this._context.SpendingLimits
            .FirstOrDefaultAsync(l =>
                l.WalletId == walletId &&
                l.CategoryName == categoryName &&
                l.Month == month &&
                l.Year == year);
    }

    public async Task<decimal> GetSpentAmountForMonthAsync(Guid walletId, string categoryName, int month, int year)
    {
        return await this._context.Transactions
            .Where(t =>
                t.WalletId == walletId &&
                t.CategoryName == categoryName &&
                t.TransactionType == TransactionType.Expense &&
                t.Date.Month == month &&
                t.Date.Year == year)
            .SumAsync(t => t.Amount);
    }

    public async Task AddAsync(SpendingLimit spendingLimit)
    {
        await this._context.SpendingLimits.AddAsync(spendingLimit);
    }

    public async Task<IEnumerable<SpendingLimit>> GetLimitsForWalletAsync(Guid walletId)
    {
        return await this._context.SpendingLimits
            .Where(l => l.WalletId == walletId)
            .OrderBy(l => l.Year)
            .ThenBy(l => l.Month)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await this._context.SaveChangesAsync();
    }
}
