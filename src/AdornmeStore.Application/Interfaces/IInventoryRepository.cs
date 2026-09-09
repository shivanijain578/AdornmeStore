using AdornmeStore.Domain.Entities;

namespace AdornmeStore.Application.Interfaces;

public interface IInventoryRepository
{
    Task<Product?> GetProductForUpdateAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Product>> GetInventoryAsync(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<InventoryTransaction>> GetTransactionsAsync(
        int? productId = null,
        CancellationToken cancellationToken = default);

    Task AddTransactionAsync(
        InventoryTransaction transaction);
}