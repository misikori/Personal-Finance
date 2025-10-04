using Budget.Application.Interfaces;
using Budget.Domain.Entities;

namespace Budget.Application.SpendingLimits;

public class SpendingLimitService : ISpendingLimitService
{
    private readonly ISpendingLimitRepository _limitRepo;
    private readonly ICategoryRepository _categoryRepo;

    public SpendingLimitService(ISpendingLimitRepository limitRepo, ICategoryRepository categoryRepo)
    {
        _limitRepo = limitRepo;
        _categoryRepo = categoryRepo;
    }


    public async Task<SpendingLimit> CreateLimitAsync(CreateLimitDto createLimitDto)
    {
        var category = await this._categoryRepo.FindByNameAsync(userId: createLimitDto.UserId,
            categoryName: createLimitDto.CategoryName);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category '{createLimitDto.CategoryName}' not found for this user.");
        }

        var existingLimit = await this._limitRepo.GetLimitForMonthAsync(walletId: createLimitDto.WalletId,
            categoryName: createLimitDto.CategoryName,
            month: createLimitDto.Month, year: createLimitDto.Year);
        if (existingLimit != null)
        {
            throw new InvalidOperationException("A limit for this category already exists.");
        }

        var limit = new SpendingLimit
        {
            WalletId = createLimitDto.WalletId,
            CategoryName = category.Name,
            Amount = createLimitDto.Amount,
            Month = createLimitDto.Month,
            Year = createLimitDto.Year,
            Id = category.Id
        };

        await this._limitRepo.AddAsync(limit);
        await this._limitRepo.SaveChangesAsync();

        return limit;
    }
}
