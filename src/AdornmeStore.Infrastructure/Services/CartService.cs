using AdornmeStore.Application.DTOs.Cart;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CartService(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<CartResponseDto> GetCartAsync()
        {
            var cart =
                await GetOrCreateCartAsync();

            return MapCart(cart);
        }

        public async Task<CartResponseDto> AddItemAsync(
            AddCartItemDto dto)
        {
            if (dto.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }

            var product =
                await _productRepository.GetByIdAsync(
                    dto.ProductId);

            if (product == null || !product.IsActive)
            {
                throw new KeyNotFoundException(
                    "Product not found.");
            }

            if (product.StockQuantity < dto.Quantity)
            {
                throw new InvalidOperationException(
                    "Requested quantity is not available.");
            }

            var cart =
                await GetOrCreateCartAsync();

            var existingItem =
                await _cartRepository.GetItemAsync(
                    cart.Id,
                    dto.ProductId);

            if (existingItem != null)
            {
                var newQuantity =
                    existingItem.Quantity + dto.Quantity;

                if (newQuantity > product.StockQuantity)
                {
                    throw new InvalidOperationException(
                        "Requested quantity exceeds available stock.");
                }

                existingItem.Quantity = newQuantity;
            }
            else
            {
                var item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };

                await _cartRepository.AddItemAsync(item);
            }

            await _unitOfWork.SaveChangesAsync();

            var updatedCart =
                await _cartRepository.GetByUserIdAsync(
                    _currentUser.UserId);

            return MapCart(updatedCart!);
        }

        public async Task<CartResponseDto?> UpdateItemAsync(
            int productId,
            UpdateCartItemDto dto)
        {
            if (dto.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be greater than zero.");
            }

            var cart =
                await _cartRepository.GetByUserIdAsync(
                    _currentUser.UserId);

            if (cart == null)
                return null;

            var item =
                await _cartRepository.GetItemAsync(
                    cart.Id,
                    productId);

            if (item == null)
                return null;

            if (dto.Quantity > item.Product.StockQuantity)
            {
                throw new InvalidOperationException(
                    "Requested quantity exceeds available stock.");
            }

            item.Quantity = dto.Quantity;

            await _unitOfWork.SaveChangesAsync();

            var updatedCart =
                await _cartRepository.GetByUserIdAsync(
                    _currentUser.UserId);

            return MapCart(updatedCart!);
        }

        public async Task<bool> RemoveItemAsync(
            int productId)
        {
            var cart =
                await _cartRepository.GetByUserIdAsync(
                    _currentUser.UserId);

            if (cart == null)
                return false;

            var item =
                await _cartRepository.GetItemAsync(
                    cart.Id,
                    productId);

            if (item == null)
                return false;

            await _cartRepository.RemoveItemAsync(item);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task ClearAsync()
        {
            var cart =
                await _cartRepository.GetByUserIdAsync(
                    _currentUser.UserId);

            if (cart == null)
                return;

            foreach (var item in cart.Items.ToList())
            {
                await _cartRepository.RemoveItemAsync(item);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<Cart> GetOrCreateCartAsync()
        {
            var cart =
                await _cartRepository.GetByUserIdAsync(
                    _currentUser.UserId);

            if (cart != null)
                return cart;

            cart = new Cart
            {
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _cartRepository.AddCartAsync(cart);

            await _unitOfWork.SaveChangesAsync();

            return cart;
        }

        private static CartResponseDto MapCart(
            Cart cart)
        {
            var items = cart.Items.Select(item =>
                new CartItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity,
                    TotalPrice =
                        item.Product.Price * item.Quantity,
                    ImageUrl = item.Product.ImageUrl,
                    AvailableStock =
                        item.Product.StockQuantity
                })
                .ToList();

            return new CartResponseDto
            {
                Id = cart.Id,
                Items = items,
                TotalAmount =
                    items.Sum(x => x.TotalPrice),
                TotalItems =
                    items.Sum(x => x.Quantity)
            };
        }
    }
}
