using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.Infrastructure.Repositories;

public class OfferRepository : IOfferRepository
{
    private readonly ApplicationDbContext _context;

    public OfferRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Offer?> GetByIdAsync(int id)
    {
        return await _context.Offers
            .AsNoTracking()
            .Include(x => x.OfferProducts)
            .Include(x => x.OfferCategories)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<(IEnumerable<Offer> Items, int TotalCount)>
        GetOffersAsync(
            string? search,
            bool? isActive,
            int pageNumber,
            int pageSize)
    {
        IQueryable<Offer> query =
            _context.Offers
                .AsNoTracking()
                .Include(x => x.OfferProducts)
                .Include(x => x.OfferCategories);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Name.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == isActive.Value);
        }

        query = query
            .OrderByDescending(x => x.CreatedAt);

        var totalCount =
            await query.CountAsync();

        var items =
            await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(Offer offer)
    {
        await _context.Offers.AddAsync(offer);
    }

    public Task UpdateAsync(Offer offer)
    {
        _context.Offers.Update(offer);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Offer offer)
    {
        _context.Offers.Remove(offer);

        return Task.CompletedTask;
    }

    public async Task<bool> ProductExistsAsync(int productId)
    {
        return await _context.Products
            .AnyAsync(x => x.Id == productId);
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _context.Categories
            .AnyAsync(x => x.Id == categoryId);
    }
}