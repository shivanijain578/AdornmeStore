using AdornmeStore.API.Models;
using AdornmeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/home")]
public class HomeController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /api/home/categories
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();

        return Ok(categories);
    }

    // GET: /api/home/categories/{id}
    [HttpGet("categories/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(int id)
    {
        var c = await _db.Categories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            })
            .FirstOrDefaultAsync();

        if (c == null)
            return NotFound(new { message = "Category not found." });

        return Ok(c);
    }
}
