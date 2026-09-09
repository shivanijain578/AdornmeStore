using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetProductForUpdateAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .FirstOrDefaultAsync(
                p => p.Id == productId,
                cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetInventoryAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<InventoryTransaction>>
        GetTransactionsAsync(
            int? productId = null,
            CancellationToken cancellationToken = default)
    {
        var query = _context.InventoryTransactions
            .AsNoTracking()
            .Include(x => x.Product)
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(
                x => x.ProductId == productId.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddTransactionAsync(
        InventoryTransaction transaction)
    {
        await _context.InventoryTransactions
            .AddAsync(transaction);
    }
}