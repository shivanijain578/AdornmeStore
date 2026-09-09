using AdornmeStore.Domain.Enums;

namespace AdornmeStore.Application.DTOs.Inventory;

public class InventoryTransactionResponseDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public InventoryTransactionType Type { get; set; }

    public int Quantity { get; set; }

    public int PreviousStock { get; set; }

    public int NewStock { get; set; }

    public string? Reason { get; set; }

    public string? Reference { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}