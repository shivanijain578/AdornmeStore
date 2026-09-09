using AdornmeStore.Application.DTOs.Common;
using AdornmeStore.Application.DTOs.Products;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace AdornmeStore.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public ProductService(
     IProductRepository productRepository,
     ICategoryRepository categoryRepository,
     IUnitOfWork unitOfWork,
     IFileStorageService fileStorageService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
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

        var products = result.Items.Select(MapToDto).ToList();

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
        ValidateProduct(
            dto.OriginalPrice,
            dto.SellingPrice);

        ValidateImages(dto.Images);

        var category =
            await _categoryRepository.GetByIdAsync(dto.CategoryId);

        if (category == null)
            throw new KeyNotFoundException(
                "Category not found.");

        var product = new Product
        {
            Name = dto.Name.Trim(),

            Description = dto.Description.Trim(),

            OriginalPrice = dto.OriginalPrice,

            SellingPrice = dto.SellingPrice,

            StockQuantity = dto.StockQuantity,

            Material = string.IsNullOrWhiteSpace(dto.Material)
                ? null
                : dto.Material.Trim(),

            Gender = (Domain.Enums.Gender)dto.Gender,

            CategoryId = dto.CategoryId,

            IsActive = true,

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow
        };

        product.CalculateDiscount();

        var displayOrder = 1;

        foreach (var image in dto.Images)
        {
            var imageUrl =
                await _fileStorageService.SaveFileAsync(
                    image,
                    "product-images");

            product.ProductImages.Add(
                new ProductImage
                {
                    ImageUrl = imageUrl,

                    DisplayOrder = displayOrder++,

                    CreatedAt = DateTime.UtcNow,

                    UpdatedAt = DateTime.UtcNow
                });
        }

        await _productRepository.AddAsync(product);

        await _unitOfWork.SaveChangesAsync();

        var created =
            await _productRepository
                .GetByIdWithCategoryAsync(product.Id);

        return MapToDto(created!);
    }

    public async Task<ProductResponseDto?> UpdateAsync(
     int id,
     UpdateProductDto dto)
    {
        ValidateProduct(
            dto.OriginalPrice,
            dto.SellingPrice);

        ValidateImages(dto.Images);

        var product =
            await _productRepository.GetByIdAsync(id);

        if (product == null)
            return null;

        var category =
            await _categoryRepository.GetByIdAsync(dto.CategoryId);

        if (category == null)
            throw new KeyNotFoundException(
                "Category not found.");

        product.Name = dto.Name.Trim();

        product.Description = dto.Description.Trim();

        product.OriginalPrice = dto.OriginalPrice;

        product.SellingPrice = dto.SellingPrice;

        product.StockQuantity = dto.StockQuantity;

        product.Material = string.IsNullOrWhiteSpace(dto.Material)
            ? null
            : dto.Material.Trim();

        product.Gender =
            (Domain.Enums.Gender)dto.Gender;

        product.CategoryId = dto.CategoryId;

        product.IsActive = dto.IsActive;

        product.UpdatedAt = DateTime.UtcNow;

        product.CalculateDiscount();

        // Delete old physical image files
        foreach (var oldImage in product.ProductImages)
        {
            await _fileStorageService.DeleteFileAsync(
                oldImage.ImageUrl);
        }

        // Remove old ProductImage records
        product.ProductImages.Clear();

        // Save new images
        var displayOrder = 1;

        foreach (var image in dto.Images)
        {
            var imageUrl =
                await _fileStorageService.SaveFileAsync(
                    image,
                    "product-images");

            product.ProductImages.Add(
                new ProductImage
                {
                    ProductId = product.Id,

                    ImageUrl = imageUrl,

                    DisplayOrder = displayOrder++,

                    CreatedAt = DateTime.UtcNow,

                    UpdatedAt = DateTime.UtcNow
                });
        }

        await _productRepository.UpdateAsync(product);

        await _unitOfWork.SaveChangesAsync();

        var updated =
            await _productRepository
                .GetByIdWithCategoryAsync(id);

        return MapToDto(updated!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product =
            await _productRepository.GetByIdAsync(id);

        if (product == null)
            return false;

        // Keep existing soft-delete behaviour.
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static void ValidateProduct(
        decimal originalPrice,
        decimal sellingPrice)
    {
        if (originalPrice <= 0)
            throw new ArgumentException(
                "Original price must be greater than zero.");

        if (sellingPrice <= 0)
            throw new ArgumentException(
                "Selling price must be greater than zero.");

        if (sellingPrice > originalPrice)
            throw new ArgumentException(
                "Selling price cannot be greater than original price.");
    }

    private static void ValidateImages(
    List<IFormFile> images)
    {
        if (images == null || images.Count == 0)
            throw new ArgumentException(
                "At least one product image is required.");

        if (images.Count > 3)
            throw new ArgumentException(
                "A maximum of 3 product images is allowed.");

        if (images.Any(x => x == null || x.Length == 0))
            throw new ArgumentException(
                "Product images cannot be empty.");
    }

    private static ProductResponseDto MapToDto(
        Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,

            Name = product.Name,

            Description = product.Description,

            OriginalPrice = product.OriginalPrice,

            SellingPrice = product.SellingPrice,

            DiscountPercentage = product.DiscountPercentage,

            StockQuantity = product.StockQuantity,

            Material = product.Material,

            Gender = (int)product.Gender,

            CategoryId = product.CategoryId,

            CategoryName = product.Category?.Name ?? string.Empty,

            IsActive = product.IsActive,

            CreatedAt = product.CreatedAt,

            UpdatedAt = product.UpdatedAt,

            Images = product.ProductImages
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new ProductImageResponseDto
                {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList()
        };
    }
}