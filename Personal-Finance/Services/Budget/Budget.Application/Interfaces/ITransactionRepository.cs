using Budget.Domain.Entities;

namespace Budget.Application.Interfaces
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);
        Task SaveChangesAsync();

    }
}
