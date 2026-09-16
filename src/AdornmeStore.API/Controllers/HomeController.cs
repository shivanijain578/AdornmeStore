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

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetHome(CancellationToken cancellationToken)
    {
        var banners = await _db.Banners.AsNoTracking().Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder).ThenByDescending(b => b.CreatedAt)
            .Select(b => new { id = b.Id, title = b.Title, imageUrl = b.ImageUrl, linkUrl = b.LinkUrl, displayOrder = b.DisplayOrder })
            .ToListAsync(cancellationToken);

        var categories = await GetVisibleCategories(cancellationToken);

        var bestSellerProducts = await _db.OrderItems.AsNoTracking()
            .Where(item => item.Order.Status != Domain.Enums.OrderStatus.Cancelled && item.Order.Status != Domain.Enums.OrderStatus.Returned && item.ProductId > 0)
            .GroupBy(item => new { item.ProductId, item.ProductName })
            .Select(g => new { productId = g.Key.ProductId, productName = g.Key.ProductName, quantitySold = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.quantitySold).Take(8).ToListAsync(cancellationToken);

        var bestSellerIds = bestSellerProducts.Select(x => x.productId).ToList();
        var bestSellerProductsData = await _db.Products.AsNoTracking()
            .Where(p => p.IsVisible && bestSellerIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name, p.OriginalPrice, p.SellingPrice, p.DiscountPercentage, p.CategoryId, categoryName = p.Category.Name,
                images = p.ProductImages.OrderBy(i => i.DisplayOrder).Select(i => new { i.Id, i.ImageUrl, i.DisplayOrder }).ToList() })
            .ToListAsync(cancellationToken);

        var bestSellers = bestSellerProducts.Join(bestSellerProductsData, x => x.productId, x => x.Id,
            (sales, product) => new { product.Id, product.Name, product.OriginalPrice, product.SellingPrice, product.DiscountPercentage, product.CategoryId, product.categoryName, product.images, sales.quantitySold })
            .OrderByDescending(x => x.quantitySold).ToList();

        var newArrivals = await _db.Products.AsNoTracking().Where(p => p.IsVisible).OrderByDescending(p => p.CreatedAt).Take(8)
            .Select(p => new { p.Id, p.Name, p.OriginalPrice, p.SellingPrice, p.DiscountPercentage, p.CategoryId, categoryName = p.Category.Name,
                images = p.ProductImages.OrderBy(i => i.DisplayOrder).Select(i => new { i.Id, i.ImageUrl, i.DisplayOrder }).ToList() })
            .ToListAsync(cancellationToken);

        var topStyles = bestSellers.Count > 0 ? bestSellers : newArrivals;
        return Ok(new { banners, categories, bestSellers = topStyles, newArrivals });
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        return Ok(await GetVisibleCategories(cancellationToken));
    }

    [HttpGet("categories/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(int id, CancellationToken cancellationToken)
    {
        var category = await _db.Categories.AsNoTracking()
            .Where(c => c.Id == id && c.IsVisible)
            .Select(c => new { id = c.Id, name = c.Name, description = c.Description, imageUrl = c.ImageUrl })
            .FirstOrDefaultAsync(cancellationToken);

        return category == null ? NotFound(new { message = "Category not found." }) : Ok(category);
    }

    private async Task<List<object>> GetVisibleCategories(CancellationToken cancellationToken)
    {
        // Prefer the admin-uploaded category image. If older categories do not have one,
        // use the first visible product image from the same category so the storefront
        // remains visual without introducing any static placeholder data.
        var categories = await _db.Categories.AsNoTracking().Where(c => c.IsVisible).OrderBy(c => c.Name)
            .Select(c => new { id = c.Id, name = c.Name, description = c.Description, imageUrl = c.ImageUrl,
                fallbackImageUrl = c.Products.Where(p => p.IsVisible).SelectMany(p => p.ProductImages.OrderBy(i => i.DisplayOrder)).Select(i => i.ImageUrl).FirstOrDefault() })
            .ToListAsync(cancellationToken);

        return categories.Select(c => (object)new { c.id, c.name, c.description, imageUrl = string.IsNullOrWhiteSpace(c.imageUrl) ? c.fallbackImageUrl : c.imageUrl }).ToList();
    }
}