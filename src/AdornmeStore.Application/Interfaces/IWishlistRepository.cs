using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AdornmeStore.Domain.Entities;

namespace AdornmeStore.Application.Interfaces
{
    public interface IWishlistRepository
    {
        Task<IEnumerable<WishlistItem>> GetByUserIdAsync(
            int userId);

        Task<WishlistItem?> GetAsync(
            int userId,
            int productId);

        Task AddAsync(WishlistItem wishlist);

        Task RemoveAsync(WishlistItem wishlist);
    }
}
