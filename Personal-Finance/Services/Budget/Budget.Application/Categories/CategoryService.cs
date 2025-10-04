using Budget.Application.Interfaces;
using Budget.Domain.Entities;

namespace Budget.Application.Categories;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        this._categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
    }


    public async Task<Category> CreateCategoryAsync(CategoryDto categoryDto)
    {
        var existingCategory =
            await this._categoryRepository.FindByNameAsync(userId: categoryDto.UserId, categoryName: categoryDto.Name);
        if (existingCategory != null)
        {
            throw new InvalidOperationException($"A category named '{categoryDto.Name}' already exists.");
        }

        var category = new Category
        {
            UserId = categoryDto.UserId,
            Name = categoryDto.Name.Trim()
        };

        await this._categoryRepository.AddAsync(category);
        await this._categoryRepository.SaveChangesAsync();

        return category;
    }
}
