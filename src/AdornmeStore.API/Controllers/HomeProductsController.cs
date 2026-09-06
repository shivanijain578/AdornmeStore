using AdornmeStore.API.Models;
using AdornmeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/home/products")]
public class HomeProductsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public HomeProductsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /api/home/products
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(
        [FromQuery] int? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _db.Products.AsNoTracking()
            .Where(p => p.IsActive);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .ToListAsync();

        return Ok(items);
    }

    // GET: /api/home/products/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new ProductDetailDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                DiscountPrice = x.DiscountPrice,
                StockQuantity = x.StockQuantity,
                ImageUrl = x.ImageUrl,
                Material = x.Material,
                Gender = x.Gender,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name
            })
            .FirstOrDefaultAsync();

        if (p == null)
            return NotFound(new { message = "Product not found." });

        return Ok(p);
    }
}
