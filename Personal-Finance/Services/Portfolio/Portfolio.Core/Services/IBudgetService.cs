namespace Portfolio.Core.Services;

/// <summary>
/// Interface for Budget service integration
/// The Budget service handles user cash balances, deposits, and withdrawals
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Checks if wallet has sufficient funds for a purchase
    /// </summary>
    Task<bool> HasSufficientFundsAsync(string walletId, decimal amount);

    /// <summary>
    /// Deducts amount from wallet when buying stocks
    /// Returns false if insufficient funds
    /// </summary>
    Task<bool> DeductFromBudgetAsync(string userId, string walletId, decimal amount, string currency);

    /// <summary>
    /// Adds amount to wallet when selling stocks
    /// </summary>
    Task<bool> AddToBudgetAsync(string userId, string walletId, decimal amount, string currency);
}

