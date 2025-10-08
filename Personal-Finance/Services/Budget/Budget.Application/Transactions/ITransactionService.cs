using Budget.Domain.Entities;

namespace Budget.Application.Transactions
{
    public interface ITransactionService
    {
        Task<Transaction?> CreateTransactionAsync(CreateTransactionDto transactionDto);
    }
}
