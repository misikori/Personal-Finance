using Budget.Application.Interfaces;
using Budget.Domain.Entities;


namespace Budget.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository(BudgetDbContext context) : ITransactionRepository
    {
        private readonly BudgetDbContext _context = context;

        public async Task AddAsync(Transaction transaction) => _ = await this._context.Transactions.AddAsync(transaction);

        public async Task SaveChangesAsync() => _ = await this._context.SaveChangesAsync();
    }
}
