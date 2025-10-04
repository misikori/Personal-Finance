using Budget.Domain.Entities;

namespace Budget.Application.Categories;

public interface ICategoryService
{
    Task<Category> CreateCategoryAsync(CategoryDto categoryDto);
}
