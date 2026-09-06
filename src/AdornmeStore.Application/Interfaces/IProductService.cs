using AdornmeStore.Application.DTOs.Common;
using AdornmeStore.Application.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface IProductService
    {
        Task<PagedResult<ProductResponseDto>> GetProductsAsync(
            ProductQueryDto query);

        Task<ProductResponseDto?> GetByIdAsync(int id);

        Task<ProductResponseDto> CreateAsync(
            CreateProductDto dto);

        Task<ProductResponseDto?> UpdateAsync(
            int id,
            UpdateProductDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
