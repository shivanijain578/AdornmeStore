using AdornmeStore.Application.DTOs.Products;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Domain.Enums;
using AdornmeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.Application.Services;

public class ProductPricingService : IProductPricingService
{
    private readonly ApplicationDbContext _context;

    public ProductPricingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductPricingDto> GetProductPricingAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(
                p => p.Id == productId,
                cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException(
                $"Product with ID {productId} was not found.");
        }

        var now = DateTime.UtcNow;

        var offers = await _context.Offers
            .Include(o => o.OfferProducts)
            .Include(o => o.OfferCategories)
            .Where(o =>
                o.IsActive &&
                o.StartDate <= now &&
                o.EndDate >= now &&
                (
                    o.Scope == OfferScope.Store ||

                    (o.Scope == OfferScope.Products &&
                     o.OfferProducts.Any(op =>
                         op.ProductId == productId)) ||

                    (o.Scope == OfferScope.Categories &&
                     o.OfferCategories.Any(oc =>
                         oc.CategoryId == product.CategoryId))
                ))
            .ToListAsync(cancellationToken);

        var applicableOffer = SelectBestOffer(offers, product.SellingPrice);

        var sellingPrice = product.SellingPrice;
        var finalPrice = sellingPrice;
        decimal offerDiscount = 0;

        if (applicableOffer != null)
        {
            offerDiscount = CalculateOfferDiscount(
                sellingPrice,
                applicableOffer);

            finalPrice = sellingPrice - offerDiscount;
        }

        return new ProductPricingDto
        {
            OriginalPrice = product.OriginalPrice,
            SellingPrice = sellingPrice,
            DiscountPercentage = product.DiscountPercentage,
            OfferDiscount = offerDiscount,
            FinalPrice = finalPrice,
            OfferId = applicableOffer?.Id,
            OfferName = applicableOffer?.Name
        };
    }

    private static Offer? SelectBestOffer(IEnumerable<Offer> offers,decimal sellingPrice)
    {
        return offers
            .OrderByDescending(o => GetScopePriority(o.Scope))
            .ThenByDescending(o =>
                CalculateOfferDiscount(sellingPrice, o))
            .ThenByDescending(o => o.Id)
            .FirstOrDefault();
    }
    private static int GetScopePriority(OfferScope scope)
    {
        return scope switch
        {
            OfferScope.Products => 3,
            OfferScope.Categories => 2,
            OfferScope.Store => 1,
            _ => 0
        };
    }

    private static decimal CalculatePotentialDiscount(
        Offer offer,
        int productId,
        int categoryId)
    {
        // Priority is handled first by scope.
        // This value is used only when comparing
        // offers having the same scope.

        return offer.OfferType switch
        {
            OfferType.Percentage => offer.DiscountValue,
            OfferType.Flat => offer.DiscountValue,
            _ => 0
        };
    }

    private static decimal CalculateOfferDiscount(
        decimal sellingPrice,
        Offer offer)
    {
        decimal discount;

        if (offer.OfferType == OfferType.Percentage)
        {
            discount = sellingPrice *
                       (offer.DiscountValue / 100m);
        }
        else
        {
            discount = offer.DiscountValue;
        }

        // Never allow the offer to make the
        // final price negative.
        return Math.Min(discount, sellingPrice);
    }
}