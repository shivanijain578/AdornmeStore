using AdornmeStore.Application.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync();

        Task<CartResponseDto> AddItemAsync(
            AddCartItemDto dto);

        Task<CartResponseDto?> UpdateItemAsync(
            int productId,
            UpdateCartItemDto dto);

        Task<bool> RemoveItemAsync(
            int productId);

        Task ClearAsync();
    }
}
