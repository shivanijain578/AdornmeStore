using AdornmeStore.API.Models;
using AdornmeStore.Application.DTOs.Orders;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Enums;
using AdornmeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly ApplicationDbContext _db;
    private readonly IInventoryService _inventoryService;

    public OrdersController(
     ICurrentUserService currentUser,
     ApplicationDbContext db,
     IInventoryService inventoryService)
    {
        _currentUser = currentUser;
        _db = db;
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == _currentUser.UserId)
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .Include(o => o.Address)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var result = orders.Select(o => new OrderResponseDto
        {
            Id = o.Id,
            TotalAmount = o.TotalAmount,
            Status = o.Status.ToString(),
            CreatedAt = o.CreatedAt,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            Address = o.Address != null ? $"{o.Address.AddressLine1}, {o.Address.City}, {o.Address.State} {o.Address.PostalCode}" : string.Empty,
            Payment = o.Payment != null ? new PaymentMethodDto
            {
                Id = o.Payment.Id,
                CardHolderName = o.Payment.PaymentMethod,
                CardLast4 = o.Payment.TransactionId ?? string.Empty,
                CardType = o.Payment.PaymentMethod,
                ExpiryMonth = 0,
                ExpiryYear = 0,
                IsDefault = true
            } : null
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var o = await _db.Orders
            .AsNoTracking()
            .Where(x => x.UserId == _currentUser.UserId && x.Id == id)
            .Include(x => x.Items)
            .Include(x => x.Payment)
            .Include(x => x.Address)
            .FirstOrDefaultAsync();

        if (o == null)
            return NotFound();

        var dto = new OrderResponseDto
        {
            Id = o.Id,
            TotalAmount = o.TotalAmount,
            Status = o.Status.ToString(),
            CreatedAt = o.CreatedAt,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            Address = o.Address != null ? $"{o.Address.AddressLine1}, {o.Address.City}, {o.Address.State} {o.Address.PostalCode}" : string.Empty,
            Payment = o.Payment != null ? new PaymentMethodDto
            {
                Id = o.Payment.Id,
                CardHolderName = o.Payment.PaymentMethod,
                CardLast4 = o.Payment.TransactionId ?? string.Empty,
                CardType = o.Payment.PaymentMethod,
                ExpiryMonth = 0,
                ExpiryYear = 0,
                IsDefault = true
            } : null
        };

        return Ok(dto);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelOrder(
    int id,
    CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(
                o => o.Id == id &&
                     o.UserId == _currentUser.UserId,
                cancellationToken);

        if (order == null)
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }

        // Only Pending orders can be cancelled.
        if (order.Status != OrderStatus.Pending)
        {
            return BadRequest(new
            {
                message =
                    "An order can only be cancelled while it is pending."
            });
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            foreach (var item in order.Items)
            {
                await _inventoryService.RecordStockInAsync(
                    item.ProductId,
                    item.Quantity,
                    "Order cancelled",
                    $"CANCEL-{order.Id}",
                    _currentUser.UserId,
                    cancellationToken);
            }

            order.Status = OrderStatus.Cancelled;

            await _db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Order cancelled successfully.",
                orderId = order.Id,
                status = order.Status.ToString()
            });
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    // =====================================================
    // CUSTOMER - ORDER INVOICE
    // =====================================================

    [HttpGet("{id:int}/invoice")]
    public async Task<IActionResult> GetInvoice(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .Include(o => o.Address)
            .Include(o => o.User)
            .FirstOrDefaultAsync(
                o => o.Id == id &&
                     o.UserId == _currentUser.UserId,
                cancellationToken);

        if (order == null)
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }


        var invoice = new
        {
            invoiceNumber = $"INV-{order.Id:D6}",

            orderId = order.Id,

            orderDate = order.CreatedAt,

            status = order.Status.ToString(),

            customer = new
            {
                name = order.User != null
                    ? $"{order.User.FirstName} {order.User.LastName}".Trim()
                    : string.Empty,

                email = order.User?.Email
            },

            billingAddress = order.Address == null
                ? null
                : new
                {
                    fullName = order.Address.FullName,
                    phoneNumber = order.Address.PhoneNumber,
                    addressLine1 = order.Address.AddressLine1,
                    addressLine2 = order.Address.AddressLine2,
                    city = order.Address.City,
                    state = order.Address.State,
                    postalCode = order.Address.PostalCode
                },

            items = order.Items.Select(item => new
            {
                productId = item.ProductId,
                productName = item.ProductName,
                quantity = item.Quantity,
                unitPrice = item.UnitPrice,
                total = item.UnitPrice * item.Quantity
            }).ToList(),

            subtotal = order.Items.Sum(
                item => item.UnitPrice * item.Quantity),

            totalAmount = order.TotalAmount,

            payment = order.Payment == null
                ? null
                : new
                {
                    method = order.Payment.PaymentMethod,
                    status = order.Payment.Status.ToString(),
                    transactionId = order.Payment.TransactionId,
                    paidAt = order.Payment.PaidAt
                }
        };

        return Ok(invoice);
    }

    // =====================================================
    // ADMIN - GET ALL ORDERS
    // =====================================================

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] OrderStatus? status = null)
    {
        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .Include(o => o.Address)
            .Include(o => o.User)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var result = orders.Select(o => new
        {
            id = o.Id,
            userId = o.UserId,
            customerEmail = o.User?.Email,
            totalAmount = o.TotalAmount,
            status = o.Status.ToString(),
            createdAt = o.CreatedAt,

            address = o.Address != null
                ? $"{o.Address.AddressLine1}, {o.Address.City}, {o.Address.State} {o.Address.PostalCode}"
                : string.Empty,

            itemCount = o.Items.Sum(i => i.Quantity),

            items = o.Items.Select(i => new
            {
                productId = i.ProductId,
                productName = i.ProductName,
                quantity = i.Quantity,
                unitPrice = i.UnitPrice
            }).ToList(),

            payment = o.Payment != null
                ? new
                {
                    id = o.Payment.Id,
                    amount = o.Payment.Amount,
                    method = o.Payment.PaymentMethod,
                    status = o.Payment.Status.ToString(),
                    transactionId = o.Payment.TransactionId,
                    paidAt = o.Payment.PaidAt
                }
                : null
        });

        return Ok(result);
    }

    // =====================================================
    // ADMIN - GET ORDER BY ID
    // =====================================================

    [HttpGet("admin/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminOrder(int id)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .Include(o => o.Address)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }

        var result = new
        {
            id = order.Id,

            customer = new
            {
                userId = order.UserId,
                email = order.User?.Email
            },

            totalAmount = order.TotalAmount,

            status = order.Status.ToString(),

            createdAt = order.CreatedAt,

            address = order.Address != null
                ? new
                {
                    order.Address.AddressLine1,
                    order.Address.City,
                    order.Address.State,
                    order.Address.PostalCode
                }
                : null,

            items = order.Items.Select(item => new
            {
                productId = item.ProductId,
                productName = item.ProductName,
                quantity = item.Quantity,
                unitPrice = item.UnitPrice,
                total = item.UnitPrice * item.Quantity
            }).ToList(),

            payment = order.Payment != null
                ? new
                {
                    id = order.Payment.Id,
                    amount = order.Payment.Amount,
                    method = order.Payment.PaymentMethod,
                    status = order.Payment.Status.ToString(),
                    transactionId = order.Payment.TransactionId,
                    paidAt = order.Payment.PaidAt
                }
                : null
        };

        return Ok(result);
    }

    // =====================================================
    // ADMIN - UPDATE ORDER STATUS
    // =====================================================

    [HttpPatch("admin/{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null)
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }

        var currentStatus = order.Status;
        var newStatus = dto.Status;

        if (!IsValidTransition(currentStatus, newStatus))
        {
            return BadRequest(new
            {
                message = $"Order cannot move from {currentStatus} to {newStatus}."
            });
        }

        // Cancellation requires inventory restoration
        if (newStatus == OrderStatus.Cancelled)
        {
            var items = await _db.OrderItems
                .Where(x => x.OrderId == order.Id)
                .ToListAsync(cancellationToken);

            await using var transaction =
                await _db.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable,
                    cancellationToken);

            try
            {
                foreach (var item in items)
                {
                    await _inventoryService.RecordStockInAsync(
                        item.ProductId,
                        item.Quantity,
                        "Order cancelled by admin",
                        $"CANCEL-{order.Id}",
                        _currentUser.UserId,
                        cancellationToken);
                }

                order.Status = OrderStatus.Cancelled;

                await _db.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return Ok(new
                {
                    orderId = order.Id,
                    previousStatus = currentStatus.ToString(),
                    status = order.Status.ToString(),
                    message = "Order cancelled and inventory restored."
                });
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        // Normal status update
        order.Status = newStatus;

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            orderId = order.Id,
            previousStatus = currentStatus.ToString(),
            status = order.Status.ToString(),
            message = "Order status updated successfully."
        });
    }
    private static bool IsValidTransition(
    OrderStatus current,
    OrderStatus next)
    {
        return current switch
        {
            OrderStatus.Pending =>
                next == OrderStatus.Confirmed ||
                next == OrderStatus.Cancelled,

            OrderStatus.Confirmed =>
                next == OrderStatus.Shipped,

            OrderStatus.Shipped =>
                next == OrderStatus.Delivered ||
                next == OrderStatus.Returned,

            OrderStatus.Delivered =>
                next == OrderStatus.Returned,

            OrderStatus.Cancelled => false,

            OrderStatus.Returned => false,

            _ => false
        };
    }
}
