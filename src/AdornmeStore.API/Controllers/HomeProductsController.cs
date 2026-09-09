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

    public HomeProductsController(
        ApplicationDbContext db)
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
        if (page < 1)
            page = 1;

        if (pageSize < 1)
            pageSize = 20;

        if (pageSize > 100)
            pageSize = 100;

        var query = _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (categoryId.HasValue)
        {
            query = query.Where(
                p => p.CategoryId == categoryId.Value);
        }

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,

                p.OriginalPrice,
                p.SellingPrice,
                p.DiscountPercentage,

                p.CategoryId,

                CategoryName = p.Category.Name,

                Images = p.ProductImages
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new
                    {
                        i.Id,
                        i.ImageUrl,
                        i.DisplayOrder
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(items);
    }

    // GET: /api/home/products/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.IsActive)
            .Select(x => new
            {
                x.Id,

                x.Name,

                x.Description,

                x.OriginalPrice,
                x.SellingPrice,
                x.DiscountPercentage,

                x.StockQuantity,

                x.Material,

                x.Gender,

                x.CategoryId,

                CategoryName = x.Category.Name,

                Images = x.ProductImages
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new
                    {
                        i.Id,
                        i.ImageUrl,
                        i.DisplayOrder
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        return Ok(product);
    }
}