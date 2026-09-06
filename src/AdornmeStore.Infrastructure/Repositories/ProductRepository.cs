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
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        return await _context.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsAsync(
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
            .Include(x => x.Category);

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.Contains(search) ||
                x.Description.Contains(search));
        }

        // Category filter
        if (categoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId == categoryId.Value);
        }

        // Minimum price
        if (minPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price >= minPrice.Value);
        }

        // Maximum price
        if (maxPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price <= maxPrice.Value);
        }

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "price" => sortDescending
                ? query.OrderByDescending(x => x.Price)
                : query.OrderBy(x => x.Price),

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