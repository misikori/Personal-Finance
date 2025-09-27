namespace Budget.Application.Transactions
{
    public interface ITransactionService
    {
        Task CreateTransactionAsync(CreateTransactionDto transactionDto);
    }
}
