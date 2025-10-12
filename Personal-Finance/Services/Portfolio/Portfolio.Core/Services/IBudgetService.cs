namespace Portfolio.Core.Services;

/// <summary>
/// Interface for Budget service integration
/// The Budget service handles user cash balances, deposits, and withdrawals
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Checks if user has sufficient funds for a purchase
    /// </summary>
    Task<bool> HasSufficientFundsAsync(string userId, decimal amount, string currency);

    /// <summary>
    /// Deducts amount from user's budget when buying stocks
    /// Returns false if insufficient funds
    /// </summary>
    Task<bool> DeductFromBudgetAsync(string userId, decimal amount, string currency);

    /// <summary>
    /// Adds amount to user's budget when selling stocks
    /// </summary>
    Task<bool> AddToBudgetAsync(string userId, decimal amount, string currency);
}

