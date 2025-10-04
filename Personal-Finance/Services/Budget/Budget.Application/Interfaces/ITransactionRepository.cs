using Budget.Application.Queries;
using Budget.Domain.Entities;

namespace Budget.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);

        Task<IEnumerable<Transaction>> GetForWalletAsync(Guid walletId, DateTime? startDate, DateTime? endDate,
            string? categoryName);

        Task<IEnumerable<Transaction>> GetExpenseTransactionsMonthAsync(Guid walletId, int month, int year);
        Task SaveChangesAsync();

    }
}
