using Budget.Application.Categories;
using Budget.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Budget.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;
    public CategoriesController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserCategories([FromQuery] Guid userId)
    {
        var categories = await this._categoryRepository.GetByUserAsync(userId: userId);
        var dtos = categories.Select(c => new CategoryDto(Id: c.Id, Name: c.Name));
        return Ok(dtos);
    }
}
