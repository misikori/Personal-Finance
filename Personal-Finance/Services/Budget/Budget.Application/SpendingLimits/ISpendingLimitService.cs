using Budget.Domain.Entities;

namespace Budget.Application.SpendingLimits;

public interface ISpendingLimitService
{
    Task<SpendingLimit> CreateLimitAsync(CreateLimitDto createLimitDto);
}
