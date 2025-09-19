using Budget.Application.Interfaces;
using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Budget.Infrastructure.Persistence.Repositories
{
    public class RecurringTransactionRepository : IRecurringTransactionRepository
    {
        private readonly BudgetDbContext _context;

        public RecurringTransactionRepository(BudgetDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RecurringTransaction recurringTransaction)
        {
            await _context.RecurringTransactions.AddAsync(recurringTransaction);
        }

        public async Task<IEnumerable<RecurringTransaction>> GetByUserIdAsync(Guid userId)
        {
            return await _context.RecurringTransactions
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


    }
}
