using Budget.Application.Interfaces;
using EventBus.Messages.Events;
using TransactionType = EventBus.Messages.Events.TransactionType;

namespace Budget.Application.Reports;

public class ReportService : IReportService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IMessagePublisher _messagePublisher;

    public ReportService(ITransactionRepository transactionRepository, IWalletRepository walletRepository,
        IMessagePublisher messagePublisher)
    {
        this._transactionRepository =
            transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        this._walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
        this._messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
    }

    public async Task GenerateTransactionReport(GenerateReportRequest request)
    {
        var wallet = await this._walletRepository.GetByIdAsync(walletId: request.WalletId);
        if (wallet == null || wallet.UserId != request.UserId)
        {
            throw new KeyNotFoundException("Wallet not found or access denied.");
        }

        var transactions = await this._transactionRepository.GetForWalletAsync(
            walletId: request.WalletId, startDate: request.StartDate, endDate: request.EndDate, categoryName: null);

        var reportItems = transactions.Select(t => new TransactionItem
        {
            Date = t.Date,
            Description = t.Description,
            CategoryName = t.CategoryName,
            TransactionType = (TransactionType)t.TransactionType,
            Amount = t.Amount,
            Currency = t.Currency,
            WalletName = wallet.Name

        }).ToList();

        var reportEvent = new TransactionsReportEvent
        {
            UserId = request.UserId.ToString(),
            EmailAddress = request.EmailAddress,
            UserName = request.Username,
            TransactionItems = reportItems
        };

        await this._messagePublisher.SendTransactionsReport(reportEvent);
    }
}
