namespace Portfolio.Core.Services;

/// <summary>
/// Interface for Budget service integration
/// TODO: This will be implemented when the separate Budget microservice is integrated
/// The Budget service handles user cash balances, deposits, and withdrawals
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Checks if user has sufficient funds for a purchase
    /// TODO: Will call Budget service API/gRPC endpoint
    /// </summary>
    Task<bool> HasSufficientFundsAsync(string username, decimal amount);

    /// <summary>
    /// Deducts amount from user's budget when buying stocks
    /// Returns false if insufficient funds
    /// TODO: Will call Budget service to deduct funds
    /// </summary>
    Task<bool> DeductFromBudgetAsync(string username, decimal amount);

    /// <summary>
    /// Adds amount to user's budget when selling stocks
    /// TODO: Will call Budget service to add funds
    /// </summary>
    Task<bool> AddToBudgetAsync(string username, decimal amount);
}
