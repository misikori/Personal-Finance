using Budget.Application.Interfaces;
using Budget.Domain.Entities;


namespace Budget.Application.Transactions
{
    public class TransactionService(ITransactionRepository transactionRepository, IWalletRepository walletRepository, ICurrencyConverter currencyConverter) : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        private readonly IWalletRepository _walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
        private readonly ICurrencyConverter _currencyConverter = currencyConverter ?? throw new ArgumentNullException(nameof(currencyConverter));

        public async Task CreateTransactionAsync(CreateTransactionDto transactionDto)
        {
            Wallet wallet = await this._walletRepository.GetByIdAsync(transactionDto.WalletId) ??
                throw new Exception("Wallet not found.");

            if (!Enum.TryParse(transactionDto.Type, true, out TransactionType transactionType))
            {
                throw new ArgumentException("Invalid transaction type.");
            }

            decimal amountToDebit = transactionDto.Amount;
            if (transactionDto.Currency != wallet.Currency)
            {
                amountToDebit = await this._currencyConverter.ConvertAsync(
                    fromCurrency: transactionDto.Currency,
                    toCurrency: wallet.Currency,
                    amount: transactionDto.Amount);
            }
            Transaction transaction = new()
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
            {
                wallet.CurrentBalance += amountToDebit;
            }
            else
            {
                wallet.CurrentBalance -= amountToDebit;
            }

            await this._transactionRepository.AddAsync(transaction);
            this._walletRepository.Update(wallet);

            await this._transactionRepository.SaveChangesAsync();
        }
    }
}
