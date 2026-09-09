using AdornmeStore.Application.DTOs.Common;
using AdornmeStore.Application.DTOs.Offers;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Domain.Enums;

namespace AdornmeStore.Infrastructure.Services;

public class OfferService : IOfferService
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OfferService(
        IOfferRepository offerRepository,
        IUnitOfWork unitOfWork)
    {
        _offerRepository = offerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<OfferResponseDto>> GetOffersAsync(
        OfferQueryDto query)
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

        var result =
            await _offerRepository.GetOffersAsync(
                query.Search,
                query.IsActive,
                query.PageNumber,
                query.PageSize);

        var items =
            result.Items
                .Select(MapToDto)
                .ToList();

        return new PagedResult<OfferResponseDto>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<OfferResponseDto?> GetByIdAsync(int id)
    {
        var offer =
            await _offerRepository.GetByIdAsync(id);

        if (offer == null)
            return null;

        return MapToDto(offer);
    }

    public async Task<OfferResponseDto> CreateAsync(
        CreateOfferDto dto)
    {
        ValidateOffer(
            dto.OfferType,
            dto.Scope,
            dto.DiscountValue,
            dto.StartDate,
            dto.EndDate,
            dto.ProductIds,
            dto.CategoryIds);

        await ValidateMappingsAsync(
            dto.Scope,
            dto.ProductIds,
            dto.CategoryIds);

        var offer = new Offer
        {
            Name = dto.Name.Trim(),

            OfferType = dto.OfferType,

            Scope = dto.Scope,

            DiscountValue = dto.DiscountValue,

            StartDate = dto.StartDate.ToUniversalTime(),

            EndDate = dto.EndDate.ToUniversalTime(),

            IsActive = dto.IsActive,

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow
        };

        if (dto.Scope == OfferScope.Products)
        {
            foreach (var productId in dto.ProductIds.Distinct())
            {
                offer.OfferProducts.Add(
                    new OfferProduct
                    {
                        ProductId = productId
                    });
            }
        }

        if (dto.Scope == OfferScope.Categories)
        {
            foreach (var categoryId in dto.CategoryIds.Distinct())
            {
                offer.OfferCategories.Add(
                    new OfferCategory
                    {
                        CategoryId = categoryId
                    });
            }
        }

        await _offerRepository.AddAsync(offer);

        await _unitOfWork.SaveChangesAsync();

        var created =
            await _offerRepository.GetByIdAsync(offer.Id);

        return MapToDto(created!);
    }

    public async Task<OfferResponseDto?> UpdateAsync(
        int id,
        UpdateOfferDto dto)
    {
        ValidateOffer(
            dto.OfferType,
            dto.Scope,
            dto.DiscountValue,
            dto.StartDate,
            dto.EndDate,
            dto.ProductIds,
            dto.CategoryIds);

        await ValidateMappingsAsync(
            dto.Scope,
            dto.ProductIds,
            dto.CategoryIds);

        var offer =
            await _offerRepository.GetByIdAsync(id);

        if (offer == null)
            return null;

        offer.Name = dto.Name.Trim();

        offer.OfferType = dto.OfferType;

        offer.Scope = dto.Scope;

        offer.DiscountValue = dto.DiscountValue;

        offer.StartDate =
            dto.StartDate.ToUniversalTime();

        offer.EndDate =
            dto.EndDate.ToUniversalTime();

        offer.IsActive = dto.IsActive;

        offer.UpdatedAt = DateTime.UtcNow;

        offer.OfferProducts.Clear();

        offer.OfferCategories.Clear();

        if (dto.Scope == OfferScope.Products)
        {
            foreach (var productId in dto.ProductIds.Distinct())
            {
                offer.OfferProducts.Add(
                    new OfferProduct
                    {
                        OfferId = offer.Id,
                        ProductId = productId
                    });
            }
        }

        if (dto.Scope == OfferScope.Categories)
        {
            foreach (var categoryId in dto.CategoryIds.Distinct())
            {
                offer.OfferCategories.Add(
                    new OfferCategory
                    {
                        OfferId = offer.Id,
                        CategoryId = categoryId
                    });
            }
        }

        await _offerRepository.UpdateAsync(offer);

        await _unitOfWork.SaveChangesAsync();

        var updated =
            await _offerRepository.GetByIdAsync(id);

        return MapToDto(updated!);
    }

    public async Task<bool> UpdateStatusAsync(
        int id,
        bool isActive)
    {
        var offer =
            await _offerRepository.GetByIdAsync(id);

        if (offer == null)
            return false;

        offer.IsActive = isActive;

        offer.UpdatedAt = DateTime.UtcNow;

        await _offerRepository.UpdateAsync(offer);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var offer =
            await _offerRepository.GetByIdAsync(id);

        if (offer == null)
            return false;

        await _offerRepository.DeleteAsync(offer);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task ValidateMappingsAsync(
        OfferScope scope,
        List<int> productIds,
        List<int> categoryIds)
    {
        if (scope == OfferScope.Products)
        {
            foreach (var productId in productIds.Distinct())
            {
                if (!await _offerRepository
                        .ProductExistsAsync(productId))
                {
                    throw new KeyNotFoundException(
                        $"Product with ID {productId} was not found.");
                }
            }
        }

        if (scope == OfferScope.Categories)
        {
            foreach (var categoryId in categoryIds.Distinct())
            {
                if (!await _offerRepository
                        .CategoryExistsAsync(categoryId))
                {
                    throw new KeyNotFoundException(
                        $"Category with ID {categoryId} was not found.");
                }
            }
        }
    }

    private static void ValidateOffer(
        OfferType offerType,
        OfferScope scope,
        decimal discountValue,
        DateTime startDate,
        DateTime endDate,
        List<int> productIds,
        List<int> categoryIds)
    {
        if (string.IsNullOrWhiteSpace(
                discountValue.ToString()))
        {
            throw new ArgumentException(
                "Discount value is required.");
        }

        if (offerType == OfferType.Percentage &&
            discountValue > 100)
        {
            throw new ArgumentException(
                "Percentage discount cannot exceed 100%.");
        }

        if (endDate <= startDate)
        {
            throw new ArgumentException(
                "End date must be later than start date.");
        }

        if (scope == OfferScope.Products)
        {
            if (productIds == null ||
                productIds.Count == 0)
            {
                throw new ArgumentException(
                    "At least one product is required.");
            }

            if (categoryIds != null &&
                categoryIds.Count > 0)
            {
                throw new ArgumentException(
                    "Category mappings cannot be used for a product offer.");
            }
        }

        if (scope == OfferScope.Categories)
        {
            if (categoryIds == null ||
                categoryIds.Count == 0)
            {
                throw new ArgumentException(
                    "At least one category is required.");
            }

            if (productIds != null &&
                productIds.Count > 0)
            {
                throw new ArgumentException(
                    "Product mappings cannot be used for a category offer.");
            }
        }

        if (scope == OfferScope.Store)
        {
            if ((productIds != null &&
                 productIds.Count > 0) ||
                (categoryIds != null &&
                 categoryIds.Count > 0))
            {
                throw new ArgumentException(
                    "Store-wide offers cannot contain product or category mappings.");
            }
        }
    }

    private static OfferResponseDto MapToDto(
        Offer offer)
    {
        var now = DateTime.UtcNow;

        return new OfferResponseDto
        {
            Id = offer.Id,

            Name = offer.Name,

            OfferType = offer.OfferType,

            Scope = offer.Scope,

            DiscountValue = offer.DiscountValue,

            StartDate = offer.StartDate,

            EndDate = offer.EndDate,

            IsActive = offer.IsActive,

            IsCurrentlyActive =
                offer.IsActive &&
                offer.StartDate <= now &&
                offer.EndDate >= now,

            CreatedAt = offer.CreatedAt,

            UpdatedAt = offer.UpdatedAt,

            ProductIds =
                offer.OfferProducts
                    .Select(x => x.ProductId)
                    .ToList(),

            CategoryIds =
                offer.OfferCategories
                    .Select(x => x.CategoryId)
                    .ToList()
        };
    }
}