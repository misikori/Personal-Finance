using Budget.Domain.Entities;

namespace Budget.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> FindByNameAsync(Guid userId, string categoryName);
    Task AddAsync(Category category);
    Task<IEnumerable<Category>> GetByUserAsync(Guid userId);
    Task SaveChangesAsync();
}
