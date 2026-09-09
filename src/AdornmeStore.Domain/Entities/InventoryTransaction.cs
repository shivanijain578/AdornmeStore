using AdornmeStore.Domain.Enums;

namespace AdornmeStore.Domain.Entities;

public class InventoryTransaction
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public InventoryTransactionType Type { get; set; }

    public int Quantity { get; set; }

    public int PreviousStock { get; set; }

    public int NewStock { get; set; }

    public string? Reason { get; set; }

    public string? Reference { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}