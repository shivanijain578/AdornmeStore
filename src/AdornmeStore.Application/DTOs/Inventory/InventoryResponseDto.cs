using AdornmeStore.Domain.Enums;

namespace AdornmeStore.Application.DTOs.Inventory;

public class InventoryResponseDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    public int LowStockThreshold { get; set; }

    public bool IsLowStock { get; set; }

    public bool IsOutOfStock { get; set; }
}