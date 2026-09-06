using AdornmeStore.Application.DTOs.Categories;
using AdornmeStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var categories =
            await _categoryService.GetAllAsync();

        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryDto dto)
    {
        var category =
            await _categoryService.CreateAsync(dto);

        return Ok(category);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        [FromBody] UpdateCategoryDto dto)
    {
        var category =
            await _categoryService.UpdateAsync(id, dto);

        if (category == null)
            return NotFound(new
            {
                message = "Category not found."
            });

        return Ok(category);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var deleted =
            await _categoryService.DeleteAsync(id);

        if (!deleted)
            return NotFound(new
            {
                message = "Category not found."
            });

        return NoContent();
    }
}