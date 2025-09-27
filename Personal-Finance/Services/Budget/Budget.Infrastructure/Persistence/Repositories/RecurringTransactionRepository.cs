using Budget.Application.Interfaces;
using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Budget.Infrastructure.Persistence.Repositories
{
    public class RecurringTransactionRepository(BudgetDbContext context) : IRecurringTransactionRepository
    {
        private readonly BudgetDbContext _context = context;

        public async Task AddAsync(RecurringTransaction recurringTransaction) => _ = await this._context.RecurringTransactions.AddAsync(recurringTransaction);

        public async Task<IEnumerable<RecurringTransaction>> GetByUserIdAsync(Guid userId) => await this._context.RecurringTransactions
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();

        public async Task SaveChangesAsync() => _ = await this._context.SaveChangesAsync();


    }
}
