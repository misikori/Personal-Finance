using Budget.Application.Interfaces;
using Budget.Application.Transactions;
using Budget.Domain.Entities;
using Budget.GRPC;
using Grpc.Core;

namespace Budget.GRPC.Services;

public class BudgetGrpcService : BudgetService.BudgetServiceBase
{
    private readonly ITransactionService _transactionService;
    private readonly IWalletRepository _walletRepository;
    private readonly ISpendingLimitRepository _limitRepository;
    private readonly ILogger<BudgetGrpcService> _logger;

    public BudgetGrpcService(ITransactionService transactionService, IWalletRepository walletRepository,
        ISpendingLimitRepository limitRepository, ILogger<BudgetGrpcService> logger)
    {
        this._transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        this._walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
        this._limitRepository = limitRepository ?? throw new ArgumentNullException(nameof(limitRepository));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async override Task<CreateTransactionResponse> CreateTransaction(CreateTransactionRequest request, ServerCallContext context)
    {
        try
        {
            var dto = new CreateTransactionDto(
                UserId: Guid.Parse(request.UserId),
                WalletId: Guid.Parse(request.WalletId),
                Amount: (decimal) request.Amount,
                Type: request.Type,
                Description: request.Description,
                Date: DateTime.UtcNow,
                Currency: request.Currency,
                CategoryName: request.CategoryName
            );

            var newTransaction = await this._transactionService.CreateTransactionAsync(dto);

            return new CreateTransactionResponse
            {
                Success = true,
                TransactionId = newTransaction.Id.ToString()
            };
        }
        catch (Exception e)
        {
            this._logger.LogError(e.Message, $"Error creating transaction via gRPC for User {request.UserId}");
            return new CreateTransactionResponse
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
    }

    public override Task<GetWalletStateResponse> GetWalletState(GetWalletStateRequest request, ServerCallContext context) => base.GetWalletState(request, context);
}
