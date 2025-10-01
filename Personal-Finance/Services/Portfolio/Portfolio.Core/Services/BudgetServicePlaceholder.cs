using Microsoft.Extensions.Logging;

namespace Portfolio.Core.Services;

/// <summary>
/// Placeholder implementation of IBudgetService
/// TODO: Replace this with actual Budget service integration (gRPC/REST client)
/// 
/// When the Budget service is ready, this class should be replaced with:
/// - gRPC client calling Budget service
/// - OR REST API client calling Budget service HTTP endpoints
/// - OR message queue integration
/// </summary>
public class BudgetServicePlaceholder : IBudgetService
{
    private readonly ILogger<BudgetServicePlaceholder> _logger;

    public BudgetServicePlaceholder(ILogger<BudgetServicePlaceholder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Checks if user has sufficient funds
    /// PLACEHOLDER: Currently always returns true
    /// TODO: Implement actual call to Budget service
    /// </summary>
    public Task<bool> HasSufficientFundsAsync(string username, decimal amount)
    {
        _logger.LogWarning(
            "BudgetService not integrated - HasSufficientFundsAsync always returns true. " +
            "User: {Username}, Amount: ${Amount}",
            username, amount);

        // TODO: Replace with actual Budget service call
        // Example:
        // var client = new BudgetService.BudgetServiceClient(_grpcChannel);
        // var response = await client.CheckFundsAsync(new CheckFundsRequest 
        // { 
        //     Username = username, 
        //     Amount = amount 
        // });
        // return response.HasSufficientFunds;

        return Task.FromResult(true); // Placeholder - always allow
    }

    /// <summary>
    /// Deducts amount from user's budget
    /// PLACEHOLDER: Currently always returns true (no actual deduction)
    /// TODO: Implement actual call to Budget service
    /// </summary>
    public Task<bool> DeductFromBudgetAsync(string username, decimal amount)
    {
        _logger.LogWarning(
            "BudgetService not integrated - DeductFromBudgetAsync does nothing. " +
            "User: {Username}, Amount to deduct: ${Amount}",
            username, amount);

        // TODO: Replace with actual Budget service call
        // Example:
        // var client = new BudgetService.BudgetServiceClient(_grpcChannel);
        // var response = await client.DeductFundsAsync(new DeductFundsRequest 
        // { 
        //     Username = username, 
        //     Amount = amount 
        // });
        // return response.Success;

        return Task.FromResult(true); // Placeholder - pretend it succeeded
    }

    /// <summary>
    /// Adds amount to user's budget
    /// PLACEHOLDER: Currently always returns true (no actual addition)
    /// TODO: Implement actual call to Budget service
    /// </summary>
    public Task<bool> AddToBudgetAsync(string username, decimal amount)
    {
        _logger.LogWarning(
            "BudgetService not integrated - AddToBudgetAsync does nothing. " +
            "User: {Username}, Amount to add: ${Amount}",
            username, amount);

        // TODO: Replace with actual Budget service call
        // Example:
        // var client = new BudgetService.BudgetServiceClient(_grpcChannel);
        // var response = await client.AddFundsAsync(new AddFundsRequest 
        // { 
        //     Username = username, 
        //     Amount = amount 
        // });
        // return response.Success;

        return Task.FromResult(true); // Placeholder - pretend it succeeded
    }
}

