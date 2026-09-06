using AdornmeStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(int userId);

        Task<CartItem?> GetItemAsync(
            int cartId,
            int productId);

        Task AddCartAsync(Cart cart);

        Task AddItemAsync(CartItem item);

        Task RemoveItemAsync(CartItem item);
    }
}
