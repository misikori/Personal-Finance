using Budget.Application.Categories;
using Budget.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Budget.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryService  _categoryService;
    public CategoriesController(ICategoryRepository categoryRepository, ICategoryService categoryService)
    {
        _categoryRepository = categoryRepository;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserCategories([FromQuery] Guid userId)
    {
        var categories = await this._categoryRepository.GetByUserAsync(userId: userId);
        var dtos = categories.Select(c => new CategoryDto(UserId: c.Id, Name: c.Name));
        return Ok(dtos);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryDto categoryDto)
    {
        try
        {
            var category = await this._categoryService.CreateCategoryAsync(categoryDto);
            return Ok(category);
        }
        catch (InvalidOperationException ex)
        {
            return this.Conflict(new { Message = ex.Message });
        }
    }
}
