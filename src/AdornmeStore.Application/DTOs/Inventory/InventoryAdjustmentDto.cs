using AdornmeStore.Domain.Enums;

namespace AdornmeStore.Application.DTOs.Inventory;

public class InventoryAdjustmentDto
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public InventoryTransactionType Type { get; set; }

    public string? Reason { get; set; }

    public string? Reference { get; set; }
}