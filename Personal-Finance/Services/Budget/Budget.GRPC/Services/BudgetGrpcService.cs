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
    private readonly ICurrencyConverter _currencyConverter;
    private readonly ILogger<BudgetGrpcService> _logger;

    public BudgetGrpcService(ITransactionService transactionService, IWalletRepository walletRepository,
        ISpendingLimitRepository limitRepository, ICurrencyConverter currencyConverter, 
        ILogger<BudgetGrpcService> logger)
    {
        this._transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        this._walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
        this._limitRepository = limitRepository ?? throw new ArgumentNullException(nameof(limitRepository));
        this._currencyConverter = currencyConverter ?? throw new ArgumentNullException(nameof(currencyConverter));
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
                TransactionId = newTransaction?.Id.ToString() ?? string.Empty
            };
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error creating transaction via gRPC for User {UserId}", request.UserId);
            return new CreateTransactionResponse
            {
                Success = false,
                ErrorMessage = e.Message
            };
        }
    }

    public override async Task<GetWalletStateResponse> GetWalletState(GetWalletStateRequest request, ServerCallContext context)
    {
        try
        {
            var userId = Guid.Parse(request.UserId);
            
            // Get user's wallets
            var wallets = await this._walletRepository.GetByUserIdAsync(userId);
            var walletsList = wallets.ToList();
            
            if (!walletsList.Any())
            {
                throw new RpcException(new Status(StatusCode.NotFound, 
                    $"No wallets found for user {request.UserId}"));
            }
            
            // Use first wallet as primary
            var primaryWallet = walletsList.First();
            
            // If no currency specified, use primary wallet's currency
            var requestedCurrency = string.IsNullOrEmpty(request.Currency) 
                ? primaryWallet.Currency 
                : request.Currency;
            
            // Calculate total balance in requested currency
            decimal totalBalance = 0;
            foreach (var wallet in walletsList)
            {
                decimal convertedBalance = wallet.CurrentBalance;
                if (wallet.Currency != requestedCurrency)
                {
                    convertedBalance = await this._currencyConverter.ConvertAsync(
                        wallet.Currency, 
                        requestedCurrency, 
                        wallet.CurrentBalance);
                }
                totalBalance += convertedBalance;
            }
            
            // Get spending limits for current month
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var limits = await this._limitRepository.GetLimitsForWalletAsync(primaryWallet.Id);
            
            var limitsList = new List<SpendingLimitsDetails>();
            foreach (var limit in limits.Where(l => l.Month == currentMonth && l.Year == currentYear))
            {
                var spent = await this._limitRepository.GetSpentAmountForMonthAsync(
                    primaryWallet.Id, 
                    limit.CategoryName, 
                    currentMonth, 
                    currentYear);
                    
                limitsList.Add(new SpendingLimitsDetails
                {
                    CategoryName = limit.CategoryName,
                    LimitAmount = (double)limit.Amount,
                    SpentAmount = (double)spent
                });
            }
            
            return new GetWalletStateResponse
            {
                CurrentBalance = (double)totalBalance,
                Currency = requestedCurrency,
                WalletId = primaryWallet.Id.ToString(),
                Limits = { limitsList }
            };
        }
        catch (FormatException)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, 
                "Invalid UserId format. Expected GUID."));
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error getting wallet state for User {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.Internal, 
                $"Error getting wallet state: {e.Message}"));
        }
    }
}
