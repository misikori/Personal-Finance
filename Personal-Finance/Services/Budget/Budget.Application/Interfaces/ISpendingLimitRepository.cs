using Budget.Domain.Entities;

namespace Budget.Application.Interfaces;

public interface ISpendingLimitRepository
{
    Task<SpendingLimit?> GetLimitForMonthAsync(Guid walletId, string categoryName, int month, int year);
    Task<decimal> GetSpentAmountForMonthAsync(Guid walletId, string categoryName, int month, int year);
    Task AddAsync(SpendingLimit spendingLimit);
    Task<IEnumerable<SpendingLimit>> GetLimitsForWalletAsync(Guid walletId);
    Task SaveChangesAsync();
}
