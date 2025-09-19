using Budget.Domain.Entities;

namespace Budget.Application.Interfaces
{
    public interface IRecurringTransactionRepository
    {
        Task AddAsync(RecurringTransaction recurringTransaction);
        Task<IEnumerable<RecurringTransaction>> GetByUserIdAsync(Guid userId);
        Task SaveChangesAsync();
    }
}
