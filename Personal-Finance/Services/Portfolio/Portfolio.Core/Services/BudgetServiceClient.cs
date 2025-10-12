using Budget.GRPC;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Portfolio.Core.Services;

/// <summary>
/// Real implementation of IBudgetService that connects to Budget gRPC service
/// Integrates with the Budget microservice to handle user cash balances
/// </summary>
public class BudgetServiceClient : IBudgetService
{
    private readonly BudgetService.BudgetServiceClient _grpcClient;
    private readonly IUserService _userService;
    private readonly ILogger<BudgetServiceClient> _logger;
    private readonly string _budgetServiceUrl;
    
    private const string CategoryInvestment = "Investment";
    private const string DescriptionStockPurchase = "Stock purchase via Portfolio service";
    private const string DescriptionStockSale = "Stock sale proceeds via Portfolio service";

    public BudgetServiceClient(string budgetServiceUrl, IUserService userService, ILogger<BudgetServiceClient> logger)
    {
        _budgetServiceUrl = budgetServiceUrl;
        _userService = userService;
        _logger = logger;
        
        // Create gRPC channel
        var channel = GrpcChannel.ForAddress(budgetServiceUrl, new GrpcChannelOptions
        {
            MaxRetryAttempts = 3,
            HttpHandler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                EnableMultipleHttp2Connections = true,
                ConnectTimeout = TimeSpan.FromSeconds(10)
            }
        });

        _grpcClient = new BudgetService.BudgetServiceClient(channel);
        
        _logger.LogInformation("Budget gRPC client initialized for {GrpcUrl}", budgetServiceUrl);
    }
    
    /// <summary>
    /// Checks if wallet has sufficient funds by calling GetWalletState gRPC
    /// </summary>
    public async Task<bool> HasSufficientFundsAsync(string walletId, decimal amount)
    {
        try
        {
            var request = new GetWalletStateRequest 
            { 
                WalletId = walletId
            };
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await _grpcClient.GetWalletStateAsync(request, cancellationToken: cts.Token);
            
            var hasEnough = (decimal)response.CurrentBalance >= amount;
            
            _logger.LogInformation(
                "Budget check for WalletId {WalletId}: Balance={Balance} {Currency}, Required={Required}, HasEnough={HasEnough}",
                walletId, response.CurrentBalance, response.Currency, amount, hasEnough);
            
            return hasEnough;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, 
                "gRPC error checking funds for WalletId {WalletId}. Status: {Status}",
                walletId, ex.StatusCode);
            throw new InvalidOperationException($"Budget service unavailable: {ex.Status.Detail}", ex);
        }
    }

    /// <summary>
    /// Deducts amount from wallet by creating an EXPENSE transaction
    /// Calls Budget service CreateTransaction with type="EXPENSE"
    /// </summary>
    public async Task<bool> DeductFromBudgetAsync(string userId, string walletId, decimal amount, string currency)
    {
        try
        {
            _logger.LogInformation(
                "Deducting {Amount} {Currency} from wallet {WalletId} for UserId {UserId} via Budget service",
                amount, currency, walletId, userId);

            var request = new CreateTransactionRequest
            {
                UserId = userId,
                WalletId = walletId,
                Amount = (double)amount,
                Type = "EXPENSE",
                Description = DescriptionStockPurchase,
                Currency = currency,
                CategoryName = CategoryInvestment
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await _grpcClient.CreateTransactionAsync(request, cancellationToken: cts.Token);

            if (response.Success)
            {
                _logger.LogInformation(
                    "Successfully deducted {Amount} {Currency} from wallet {WalletId}. Transaction ID: {TransactionId}",
                    amount, currency, walletId, response.TransactionId);
                return true;
            }
            else
            {
                _logger.LogWarning(
                    "Failed to deduct from wallet {WalletId}: {Error}",
                    walletId, response.ErrorMessage);
                return false;
            }
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, 
                "gRPC error deducting from wallet {WalletId}. Status: {Status}",
                walletId, ex.StatusCode);

            throw new InvalidOperationException($"Budget service unavailable: {ex.Status.Detail}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error deducting {Amount} {Currency} from wallet {WalletId}",
                amount, currency, walletId);
            throw;
        }
    }

    /// <summary>
    /// Adds amount to wallet by creating an INCOME transaction
    /// Calls Budget service CreateTransaction with type="INCOME"
    /// </summary>
    public async Task<bool> AddToBudgetAsync(string userId, string walletId, decimal amount, string currency)
    {
        try
        {
            _logger.LogInformation(
                "Adding {Amount} {Currency} to wallet {WalletId} for UserId {UserId} via Budget service",
                amount, currency, walletId, userId);

            var request = new CreateTransactionRequest
            {
                UserId = userId,
                WalletId = walletId,
                Amount = (double)amount,
                Type = "INCOME",
                Description = DescriptionStockSale,
                Currency = currency,
                CategoryName = CategoryInvestment
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await _grpcClient.CreateTransactionAsync(request, cancellationToken: cts.Token);

            if (response.Success)
            {
                _logger.LogInformation(
                    "Successfully added {Amount} {Currency} to wallet {WalletId}. Transaction ID: {TransactionId}",
                    amount, currency, walletId, response.TransactionId);
                return true;
            }
            else
            {
                _logger.LogWarning(
                    "Failed to add to wallet {WalletId}: {Error}",
                    walletId, response.ErrorMessage);
                return false;
            }
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, 
                "gRPC error adding to wallet {WalletId}. Status: {Status}",
                walletId, ex.StatusCode);
            
            throw new InvalidOperationException($"Budget service unavailable: {ex.Status.Detail}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error adding {Amount} {Currency} to wallet {WalletId}",
                amount, currency, walletId);
            throw;
        }
    }
}

