using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdornmeStore.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByUserIdAsync(
            int userId)
        {
            return await _context.Carts
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId);
        }

        public async Task<CartItem?> GetItemAsync(
            int cartId,
            int productId)
        {
            return await _context.CartItems
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.CartId == cartId &&
                    x.ProductId == productId);
        }

        public async Task AddCartAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
        }

        public async Task AddItemAsync(CartItem item)
        {
            await _context.CartItems.AddAsync(item);
        }

        public Task RemoveItemAsync(CartItem item)
        {
            _context.CartItems.Remove(item);

            return Task.CompletedTask;
        }
    }
}
