using Budget.Application.Interfaces;
using Budget.Domain.Entities;
using Budget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Budget.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly BudgetDbContext _context;

    public CategoryRepository(BudgetDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> FindByNameAsync(Guid userId, string categoryName)
    {
        return await this._context.Categories.FirstOrDefaultAsync(c => c.UserId == userId && c.Name.ToUpper() == categoryName.ToUpper());
    }

    public async Task AddAsync(Category category)
    {
        await this._context.Categories.AddAsync(category);
    }

    public async Task<IEnumerable<Category>> GetByUserAsync(Guid userId)
    {
        return await this._context.Categories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
