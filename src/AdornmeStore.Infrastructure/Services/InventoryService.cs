using AdornmeStore.Application.DTOs.Inventory;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Domain.Enums;
using AdornmeStore.Infrastructure.Repositories;

namespace AdornmeStore.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IUnitOfWork unitOfWork)
    {
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<InventoryResponseDto>>
        GetInventoryAsync(
            CancellationToken cancellationToken = default)
    {
        var products =
            await _inventoryRepository.GetInventoryAsync(
                cancellationToken);

        return products.Select(MapInventory);
    }

    public async Task<InventoryResponseDto?>
        GetProductInventoryAsync(
            int productId,
            CancellationToken cancellationToken = default)
    {
        var product =
            await _inventoryRepository.GetProductForUpdateAsync(
                productId,
                cancellationToken);

        if (product == null)
            return null;

        return MapInventory(product);
    }

    public async Task AdjustStockAsync( InventoryAdjustmentDto dto, int userId, CancellationToken cancellationToken = default)
    {
        if (dto.Quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.");

        var product = await _inventoryRepository.GetProductForUpdateAsync(
            dto.ProductId,
            cancellationToken);

        if (product == null)
            throw new KeyNotFoundException("Product not found.");

        var previousStock = product.StockQuantity;
        int newStock;

        switch (dto.Type)
        {
            case InventoryTransactionType.StockIn:
                if (dto.Quantity <= 0)
                    throw new ArgumentException(
                        "Stock-in quantity must be greater than zero.");

                newStock = previousStock + dto.Quantity;
                break;

            case InventoryTransactionType.StockOut:
                if (dto.Quantity <= 0)
                    throw new ArgumentException(
                        "Stock-out quantity must be greater than zero.");

                if (dto.Quantity > previousStock)
                    throw new InvalidOperationException(
                        $"Insufficient stock for '{product.Name}'. " +
                        $"Available stock: {previousStock}.");

                newStock = previousStock - dto.Quantity;
                break;

            case InventoryTransactionType.Adjustment:
                // For Adjustment, Quantity is the TARGET physical stock.
                newStock = dto.Quantity;
                break;

            default:
                throw new ArgumentException(
                    "Invalid inventory transaction type.");
        }

        var difference = Math.Abs(newStock - previousStock);

        product.StockQuantity = newStock;

        var transaction = new InventoryTransaction
        {
            ProductId = product.Id,
            Type = dto.Type,
            Quantity = difference,
            PreviousStock = previousStock,
            NewStock = newStock,
            Reason = dto.Reason,
            Reference = dto.Reference,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _inventoryRepository.AddTransactionAsync(transaction);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    public async Task<IEnumerable<InventoryTransactionResponseDto>> GetTransactionsAsync(int? productId = null, CancellationToken cancellationToken = default)
    {
        var transactions =
            await _inventoryRepository.GetTransactionsAsync(
                productId,
                cancellationToken);

        return transactions.Select(x =>
            new InventoryTransactionResponseDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Type = x.Type,
                Quantity = x.Quantity,
                PreviousStock = x.PreviousStock,
                NewStock = x.NewStock,
                Reason = x.Reason,
                Reference = x.Reference,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt
            });
    }

    public async Task RecordStockOutAsync(int productId, int quantity, string? reason, string? reference, int userId, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        var product =
            await _inventoryRepository.GetProductForUpdateAsync(
                productId,
                cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        if (!product.IsActive)
        {
            throw new InvalidOperationException(
                $"Product '{product.Name}' is no longer available.");
        }

        if (quantity > product.StockQuantity)
        {
            throw new InvalidOperationException(
                $"Insufficient stock for '{product.Name}'. " +
                $"Available stock: {product.StockQuantity}.");
        }

        var previousStock = product.StockQuantity;

        product.StockQuantity -= quantity;

        var transaction = new InventoryTransaction
        {
            ProductId = product.Id,
            Type = InventoryTransactionType.StockOut,
            Quantity = quantity,
            PreviousStock = previousStock,
            NewStock = product.StockQuantity,
            Reason = reason,
            Reference = reference,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _inventoryRepository.AddTransactionAsync(transaction);

        // Do NOT call SaveChangesAsync here.
        // Checkout owns the transaction and will save everything together.
    }
    private static InventoryResponseDto MapInventory(
        Product product)
    {
        return new InventoryResponseDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            StockQuantity = product.StockQuantity,
            LowStockThreshold =
                product.LowStockThreshold,

            IsLowStock =
                product.StockQuantity > 0 &&
                product.StockQuantity <=
                product.LowStockThreshold,

            IsOutOfStock =
                product.StockQuantity <= 0
        };
    }
    public async Task RecordStockInAsync(
    int productId,
    int quantity,
    string? reason,
    string? reference,
    int userId,
    CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new ArgumentException(
                "Stock quantity must be greater than zero.");

        var product = await _inventoryRepository
            .GetProductForUpdateAsync(
                productId,
                cancellationToken);

        if (product == null)
            throw new KeyNotFoundException(
                $"Product {productId} not found.");

        var previousStock = product.StockQuantity;
        var newStock = previousStock + quantity;

        product.StockQuantity = newStock;

        var transaction = new InventoryTransaction
        {
            ProductId = productId,
            Type = InventoryTransactionType.StockIn,
            Quantity = quantity,
            PreviousStock = previousStock,
            NewStock = newStock,
            Reason = reason,
            Reference = reference,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _inventoryRepository.AddTransactionAsync(transaction);
    }
}