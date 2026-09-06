using AdornmeStore.Application.DTOs.Wishlist;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface IWishlistService
    {
        Task<IEnumerable<WishlistItemDto>> GetAsync();

        Task AddAsync(int productId);

        Task<bool> RemoveAsync(int productId);

        Task<bool> ExistsAsync(int productId);
    }
}
