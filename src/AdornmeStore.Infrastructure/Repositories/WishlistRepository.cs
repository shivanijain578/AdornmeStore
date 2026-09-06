using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Infrastructure.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;

        public WishlistRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WishlistItem>>
            GetByUserIdAsync(int userId)
        {
            return await _context.WishlistItems
                .AsNoTracking()
                .Include(x => x.Product)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AddedAt)
                .ToListAsync();
        }

        public async Task<WishlistItem?> GetAsync(
            int userId,
            int productId)
        {
            return await _context.WishlistItems
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.ProductId == productId);
        }

        public async Task AddAsync(
            WishlistItem wishlist)
        {
            await _context.WishlistItems.AddAsync(wishlist);
        }

        public Task RemoveAsync(
            WishlistItem wishlist)
        {
            _context.WishlistItems.Remove(wishlist);

            return Task.CompletedTask;
        }
    }
}
