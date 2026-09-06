using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }

    public Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);

        return Task.CompletedTask;
    }
}