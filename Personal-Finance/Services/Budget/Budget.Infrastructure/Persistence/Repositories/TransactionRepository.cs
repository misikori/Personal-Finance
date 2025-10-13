using System.Linq;
using Budget.Application.Interfaces;
using Budget.Application.Queries;
using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Budget.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository(BudgetDbContext context) : ITransactionRepository
    {
        private readonly BudgetDbContext _context = context;

        public async Task AddAsync(Transaction transaction) => _ = await this._context.Transactions.AddAsync(transaction);
        public async Task<IEnumerable<Transaction>> GetForWalletAsync(Guid walletId, DateTime? startDate, DateTime? endDate, string? categoryName)
        {
            var query = this._context.Transactions
                .Where(t => t.WalletId == walletId);

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Add 1 day to make the end date inclusive (all transactions on that day)
                var inclusiveEndDate = endDate.Value.AddDays(1);
                query = query.Where(t => t.Date < inclusiveEndDate);
            }

            if (categoryName != null)
            {
                query = query.Where(t => t.CategoryName == categoryName);
            }

            query = query.OrderByDescending(t => t.Date);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetExpenseTransactionsMonthAsync(Guid walletId, int month, int year)
        {
            return await this._context.Transactions
                .Where(t => t.WalletId == walletId &&
                            t.Date.Month == month &&
                            t.Date.Year == year &&
                            t.TransactionType == TransactionType.Expense)
                .ToListAsync();
        }

        public async Task SaveChangesAsync() => _ = await this._context.SaveChangesAsync();
    }
}
