using Budget.Application.Interfaces;
using Budget.Domain.Entities;

namespace Budget.Application.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;

        public TransactionService(ITransactionRepository transactionRepository, IWalletRepository walletRepository)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
        }

        public async Task CreateTransactionAsync(CreateTransactionDto transactionDto)
        {
            var wallet = await _walletRepository.GetByIdAsync(transactionDto.WalletId) ?? 
                throw new Exception("Wallet not found.");

            if (!Enum.TryParse<TransactionType>(transactionDto.Type, true, out var transactionType))
                throw new ArgumentException("Invalid transaction type.");

            var transaction = new Transaction
            {
                UserId = transactionDto.UserId,
                WalletId = transactionDto.WalletId,
                Amount = transactionDto.Amount,
                TransactionType = transactionType,
                Description = transactionDto.Description,
                Date = transactionDto.Date,
                Currency = transactionDto.Currency,
            };

            if (transaction.TransactionType == TransactionType.Income)
                wallet.CurrentBalance += transaction.Amount;
            else
                wallet.CurrentBalance -= transaction.Amount;

            await _transactionRepository.AddAsync(transaction);
            _walletRepository.Update(wallet);

            await _transactionRepository.SaveChangesAsync();
        }
    }
}
