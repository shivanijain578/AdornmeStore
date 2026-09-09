using AdornmeStore.Domain.Enums;

namespace AdornmeStore.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Original/MRP price of the product.
    /// </summary>
    public decimal OriginalPrice { get; set; }

    /// <summary>
    /// Current selling price.
    /// </summary>
    public decimal SellingPrice { get; set; }

    /// <summary>
    /// Discount percentage calculated from OriginalPrice and SellingPrice.
    /// This value is maintained by the backend.
    /// </summary>
    public decimal DiscountPercentage { get; private set; }

    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;

    public string? Material { get; set; }

    public Gender Gender { get; set; }

    public int CategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties

    public Category Category { get; set; } = null!;

    public ICollection<ProductImage> ProductImages { get; set; } =
        new List<ProductImage>();

    public ICollection<WishlistItem> WishlistItems { get; set; } =
        new List<WishlistItem>();

    public ICollection<CartItem> CartItems { get; set; } =
        new List<CartItem>();

    public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
    = new List<InventoryTransaction>();

    /// <summary>
    /// Recalculates the product discount percentage.
    /// </summary>
    public void CalculateDiscount()
    {
        if (OriginalPrice <= 0)
        {
            DiscountPercentage = 0;
            return;
        }

        DiscountPercentage =
            Math.Round(
                ((OriginalPrice - SellingPrice) / OriginalPrice) * 100,
                2,
                MidpointRounding.AwayFromZero);
    }
}