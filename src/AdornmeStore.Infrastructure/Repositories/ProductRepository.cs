using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(x => x.ProductImages)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductImages)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)>
        GetProductsAsync(
            string? search,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy,
            bool sortDescending,
            int pageNumber,
            int pageSize)
    {
        IQueryable<Product> query = _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductImages);

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(search) ||
                x.Description.Contains(search));
        }

        // Category
        if (categoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId == categoryId.Value);
        }

        // Price filtering is based on SellingPrice
        if (minPrice.HasValue)
        {
            query = query.Where(x =>
                x.SellingPrice >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(x =>
                x.SellingPrice <= maxPrice.Value);
        }

        // Sorting
        query = sortBy?.Trim().ToLowerInvariant() switch
        {
            "price" => sortDescending
                ? query.OrderByDescending(x => x.SellingPrice)
                : query.OrderBy(x => x.SellingPrice),

            "mrp" => sortDescending
                ? query.OrderByDescending(x => x.OriginalPrice)
                : query.OrderBy(x => x.OriginalPrice),

            "discount" => sortDescending
                ? query.OrderByDescending(x => x.DiscountPercentage)
                : query.OrderBy(x => x.DiscountPercentage),

            "name" => sortDescending
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),

            "newest" => query.OrderByDescending(x => x.CreatedAt),

            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);

        return Task.CompletedTask;
    }
}