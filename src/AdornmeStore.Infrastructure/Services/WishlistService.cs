using AdornmeStore.Application.DTOs.Wishlist;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Infrastructure.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public WishlistService(
            IWishlistRepository wishlistRepository,
            IProductRepository productRepository,
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<WishlistItemDto>> GetAsync()
        {
            var items =
                await _wishlistRepository
                    .GetByUserIdAsync(_currentUser.UserId);

            return items.Select(x => new WishlistItemDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Price = x.Product.Price,
                ImageUrl = x.Product.ImageUrl,
                StockQuantity = x.Product.StockQuantity,
                AddedAt = x.AddedAt
            });
        }

        public async Task AddAsync(int productId)
        {
            var product =
                await _productRepository.GetByIdAsync(productId);

            if (product == null || !product.IsActive)
            {
                throw new KeyNotFoundException(
                    "Product not found.");
            }

            var existing =
                await _wishlistRepository.GetAsync(
                    _currentUser.UserId,
                    productId);

            if (existing != null)
            {
                return;
            }

            var wishlist = new WishlistItem
            {
                UserId = _currentUser.UserId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            };

            await _wishlistRepository.AddAsync(wishlist);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> RemoveAsync(int productId)
        {
            var wishlist =
                await _wishlistRepository.GetAsync(
                    _currentUser.UserId,
                    productId);

            if (wishlist == null)
            {
                return false;
            }

            await _wishlistRepository.RemoveAsync(wishlist);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int productId)
        {
            var item =
                await _wishlistRepository.GetAsync(
                    _currentUser.UserId,
                    productId);

            return item != null;
        }
    }
}
