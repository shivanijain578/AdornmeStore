using AdornmeStore.Application.DTOs.Common;
using AdornmeStore.Application.DTOs.Products;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<ProductResponseDto>> GetProductsAsync(
            ProductQueryDto query)
        {
            query.PageNumber = query.PageNumber < 1
                ? 1
                : query.PageNumber;

            query.PageSize = query.PageSize switch
            {
                < 1 => 10,
                > 100 => 100,
                _ => query.PageSize
            };

            var result = await _productRepository.GetProductsAsync(
                query.Search,
                query.CategoryId,
                query.MinPrice,
                query.MaxPrice,
                query.SortBy,
                query.SortDescending,
                query.PageNumber,
                query.PageSize);

            var products = result.Items.Select(MapToDto);

            return new PagedResult<ProductResponseDto>
            {
                Items = products,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = result.TotalCount
            };
        }

        public async Task<ProductResponseDto?> GetByIdAsync(int id)
        {
            var product =
                await _productRepository.GetByIdWithCategoryAsync(id);

            if (product == null || !product.IsActive)
                return null;

            return MapToDto(product);
        }

        public async Task<ProductResponseDto> CreateAsync(
            CreateProductDto dto)
        {
            var category =
                await _categoryRepository.GetByIdAsync(dto.CategoryId);

            if (category == null)
                throw new KeyNotFoundException("Category not found.");

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);

            await _unitOfWork.SaveChangesAsync();

            var created =
                await _productRepository.GetByIdWithCategoryAsync(product.Id);

            return MapToDto(created!);
        }

        public async Task<ProductResponseDto?> UpdateAsync(
            int id,
            UpdateProductDto dto)
        {
            var product =
                await _productRepository.GetByIdAsync(id);

            if (product == null)
                return null;

            var category =
                await _categoryRepository.GetByIdAsync(dto.CategoryId);

            if (category == null)
                throw new KeyNotFoundException("Category not found.");

            product.Name = dto.Name.Trim();
            product.Description = dto.Description.Trim();
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.ImageUrl = dto.ImageUrl;
            product.CategoryId = dto.CategoryId;
            product.IsActive = dto.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);

            await _unitOfWork.SaveChangesAsync();

            var updated =
                await _productRepository.GetByIdWithCategoryAsync(id);

            return MapToDto(updated!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product =
                await _productRepository.GetByIdAsync(id);

            if (product == null)
                return false;

            // Soft delete
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private static ProductResponseDto MapToDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt
            };
        }
    }
}