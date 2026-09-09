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

    // =====================================================
    // HOME PAGE
    // =====================================================

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetHome(
        CancellationToken cancellationToken)
    {
        // -------------------------
        // Active banners
        // -------------------------

        var banners = await _db.Banners
            .AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ThenByDescending(b => b.CreatedAt)
            .Select(b => new
            {
                id = b.Id,
                title = b.Title,
                imageUrl = b.ImageUrl,
                linkUrl = b.LinkUrl,
                displayOrder = b.DisplayOrder
            })
            .ToListAsync(cancellationToken);


        // -------------------------
        // Categories
        // -------------------------

        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description
            })
            .ToListAsync(cancellationToken);


        // -------------------------
        // Best Sellers
        // -------------------------

        var bestSellerProducts = await _db.OrderItems
            .AsNoTracking()
            .Where(item =>
                item.Order.Status != Domain.Enums.OrderStatus.Cancelled &&
                item.Order.Status != Domain.Enums.OrderStatus.Returned &&
                item.ProductId > 0)
            .GroupBy(item => new
            {
                item.ProductId,
                item.ProductName
            })
            .Select(g => new
            {
                productId = g.Key.ProductId,
                productName = g.Key.ProductName,
                quantitySold = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.quantitySold)
            .Take(8)
            .ToListAsync(cancellationToken);


        var bestSellerIds =
            bestSellerProducts
                .Select(x => x.productId)
                .ToList();


        var bestSellerProductsData = await _db.Products
            .AsNoTracking()
            .Where(p =>
                p.IsActive &&
                bestSellerIds.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.OriginalPrice,
                p.SellingPrice,
                p.DiscountPercentage,
                p.CategoryId,
                categoryName = p.Category.Name,

                images = p.ProductImages
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new
                    {
                        i.Id,
                        i.ImageUrl,
                        i.DisplayOrder
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);


        var bestSellers = bestSellerProducts
            .Join(
                bestSellerProductsData,
                x => x.productId,
                x => x.Id,
                (sales, product) => new
                {
                    product.Id,
                    product.Name,
                    product.OriginalPrice,
                    product.SellingPrice,
                    product.DiscountPercentage,
                    product.CategoryId,
                    product.categoryName,
                    product.images,
                    sales.quantitySold
                })
            .OrderByDescending(x => x.quantitySold)
            .ToList();


        // -------------------------
        // New Arrivals
        // -------------------------

        var newArrivals = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.OriginalPrice,
                p.SellingPrice,
                p.DiscountPercentage,
                p.CategoryId,
                categoryName = p.Category.Name,

                images = p.ProductImages
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new
                    {
                        i.Id,
                        i.ImageUrl,
                        i.DisplayOrder
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);


        return Ok(new
        {
            banners,
            categories,
            bestSellers,
            newArrivals
        });
    }


    // =====================================================
    // CATEGORIES
    // =====================================================

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(
        CancellationToken cancellationToken)
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description
            })
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }


    // =====================================================
    // CATEGORY DETAILS
    // =====================================================

    [HttpGet("categories/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(
        int id,
        CancellationToken cancellationToken)
    {
        var category = await _db.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                id = c.Id,
                name = c.Name,
                description = c.Description
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        return Ok(category);
    }
}