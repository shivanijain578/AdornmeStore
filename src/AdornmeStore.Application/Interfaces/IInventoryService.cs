using AdornmeStore.Application.DTOs.Inventory;

namespace AdornmeStore.Application.Interfaces;

public interface IInventoryService
{
    Task<IEnumerable<InventoryResponseDto>> GetInventoryAsync(CancellationToken cancellationToken = default);

    Task<InventoryResponseDto?> GetProductInventoryAsync(int productId, CancellationToken cancellationToken = default);

    Task AdjustStockAsync(InventoryAdjustmentDto dto,int userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<InventoryTransactionResponseDto>> GetTransactionsAsync(int? productId = null, CancellationToken cancellationToken = default);

    Task RecordStockOutAsync(int productId,int quantity,string? reason, string? reference, int userId, CancellationToken cancellationToken = default);
    Task RecordStockInAsync(int productId,int quantity,string? reason, string? reference,int userId, CancellationToken cancellationToken = default);
}