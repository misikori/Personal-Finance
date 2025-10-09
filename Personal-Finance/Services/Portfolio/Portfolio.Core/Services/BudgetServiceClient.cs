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
    /// Gets user's primary wallet ID from Budget service via GetWalletState gRPC
    /// </summary>
    private async Task<string> GetUserWalletIdAsync(string username, string currency)
    {
        try
        {
            // Resolve username to userId via IdentityServer
            var userId = await _userService.GetUserIdAsync(username);
            
            var request = new GetWalletStateRequest
            {
                UserId = userId,
                Currency = currency
            };
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await _grpcClient.GetWalletStateAsync(request, cancellationToken: cts.Token);
            
            if (string.IsNullOrEmpty(response.WalletId))
            {
                _logger.LogError("No wallet ID returned for user {Username} (UserId: {UserId})", username, userId);
                throw new InvalidOperationException($"User {username} has no wallets in Budget service");
            }
            
            _logger.LogDebug("Using wallet {WalletId} (Currency: {Currency}) for user {Username} (UserId: {UserId})", 
                response.WalletId, response.Currency, username, userId);
            
            return response.WalletId;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "gRPC error getting wallet for user {Username}. Status: {Status}", 
                username, ex.StatusCode);
            throw new InvalidOperationException($"Budget service gRPC error: {ex.Status.Detail}", ex);
        }
    }

    /// <summary>
    /// Checks if user has sufficient funds by calling GetWalletState gRPC
    /// Converts budget balance to the requested currency for comparison
    /// </summary>
    public async Task<bool> HasSufficientFundsAsync(string username, decimal amount, string currency)
    {
        try
        {
            // Resolve username to userId via IdentityServer
            var userId = await _userService.GetUserIdAsync(username);
            
            var request = new GetWalletStateRequest 
            { 
                UserId = userId, 
                Currency = currency
            };
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await _grpcClient.GetWalletStateAsync(request, cancellationToken: cts.Token);
            
            var hasEnough = (decimal)response.CurrentBalance >= amount;
            
            _logger.LogInformation(
                "Budget check for {Username} (UserId: {UserId}): Balance={Balance} {Currency}, Required={Required}, HasEnough={HasEnough}",
                username, userId, response.CurrentBalance, currency, amount, hasEnough);
            
            return hasEnough;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, 
                "gRPC error checking funds for {Username}. Status: {Status}",
                username, ex.StatusCode);
            throw new InvalidOperationException($"Budget service unavailable: {ex.Status.Detail}", ex);
        }
    }

    /// <summary>
    /// Deducts amount from user's budget by creating an EXPENSE transaction
    /// Calls Budget service CreateTransaction with type="EXPENSE"
    /// </summary>
    public async Task<bool> DeductFromBudgetAsync(string username, decimal amount, string currency)
    {
        try
        {
            _logger.LogInformation(
                "Deducting {Amount} {Currency} from budget for user {Username} via Budget service",
                amount, currency, username);

            // Resolve username to userId
            var userId = await _userService.GetUserIdAsync(username);
            
            // Get user's wallet ID from Budget service
            var walletId = await GetUserWalletIdAsync(username, currency);

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
                    "Successfully deducted ${Amount} from budget for {Username}. Transaction ID: {TransactionId}",
                    amount, username, response.TransactionId);
                return true;
            }
            else
            {
                _logger.LogWarning(
                    "Failed to deduct from budget for {Username}: {Error}",
                    username, response.ErrorMessage);
                return false;
            }
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, 
                "gRPC error deducting from budget for {Username}. Status: {Status}",
                username, ex.StatusCode);
            
            throw new InvalidOperationException($"Budget service unavailable: {ex.Status.Detail}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error deducting ${Amount} from budget for {Username}",
                amount, username);
            throw;
        }
    }

    /// <summary>
    /// Adds amount to user's budget by creating an INCOME transaction
    /// Calls Budget service CreateTransaction with type="INCOME"
    /// </summary>
    public async Task<bool> AddToBudgetAsync(string username, decimal amount, string currency)
    {
        try
        {
            _logger.LogInformation(
                "Adding {Amount} {Currency} to budget for user {Username} via Budget service",
                amount, currency, username);

            // Resolve username to userId
            var userId = await _userService.GetUserIdAsync(username);
            
            // Get user's wallet ID from Budget service
            var walletId = await GetUserWalletIdAsync(username, currency);

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
                    "Successfully added ${Amount} to budget for {Username}. Transaction ID: {TransactionId}",
                    amount, username, response.TransactionId);
                return true;
            }
            else
            {
                _logger.LogWarning(
                    "Failed to add to budget for {Username}: {Error}",
                    username, response.ErrorMessage);
                return false;
            }
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, 
                "gRPC error adding to budget for {Username}. Status: {Status}",
                username, ex.StatusCode);
            
            throw new InvalidOperationException($"Budget service unavailable: {ex.Status.Detail}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error adding ${Amount} to budget for {Username}",
                amount, username);
            throw;
        }
    }
}

