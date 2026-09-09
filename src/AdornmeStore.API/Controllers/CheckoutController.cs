using AdornmeStore.API.Models;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Domain.Enums;
using AdornmeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/checkout")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly ApplicationDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductPricingService _productPricingService;
    private readonly IInventoryService _inventoryService;
    public CheckoutController(
     ICurrentUserService currentUser,
     ApplicationDbContext db,
     IUnitOfWork unitOfWork,
     IProductPricingService productPricingService,
     IInventoryService inventoryService)
    {
        _currentUser = currentUser;
        _db = db;
        _unitOfWork = unitOfWork;
        _productPricingService = productPricingService;
        _inventoryService = inventoryService;
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(
    [FromBody] CheckoutRequestDto dto)
    {
        if (!_currentUser.IsAuthenticated)
            return Unauthorized();

        await using var transaction =
            await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

        try
        {
            // 1. Load user's cart
            var cart = await _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c =>
                    c.UserId == _currentUser.UserId);

            if (cart == null || !cart.Items.Any())
            {
                return BadRequest(new
                {
                    message = "Cart is empty."
                });
            }

            // 2. Validate address
            var address = await _db.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.AddressId &&
                    a.UserId == _currentUser.UserId);

            if (address == null)
            {
                return BadRequest(new
                {
                    message = "Invalid address."
                });
            }

            decimal total = 0;

            var itemPrices = new Dictionary<int, decimal>();

            // 3. Validate products, stock and calculate final prices
            foreach (var item in cart.Items)
            {
                var product = item.Product;

                if (product == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "One or more products are no longer available."
                    });
                }

                if (!product.IsActive)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Product '{product.Name}' is no longer available."
                    });
                }

                if (item.Quantity <= 0)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Invalid quantity for '{product.Name}'."
                    });
                }

                if (item.Quantity > product.StockQuantity)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Insufficient stock for '{product.Name}'. " +
                            $"Available stock: {product.StockQuantity}."
                    });
                }

                // Calculate current price including active offers
                var pricing =
                    await _productPricingService
                        .GetProductPricingAsync(item.ProductId);

                itemPrices[item.ProductId] = pricing.FinalPrice;

                total += pricing.FinalPrice * item.Quantity;
            }

            // 4. Create order
            var order = new Order
            {
                UserId = _currentUser.UserId,
                AddressId = dto.AddressId,
                TotalAmount = total,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            foreach (var item in cart.Items)
            {
                var finalPrice = itemPrices[item.ProductId];

                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = finalPrice
                });
            }

            _db.Orders.Add(order);

            // Save order first so Order.Id is generated
            await _unitOfWork.SaveChangesAsync();

            // 5. Deduct stock + create inventory history
            foreach (var item in cart.Items)
            {
                await _inventoryService.RecordStockOutAsync(
                    item.ProductId,
                    item.Quantity,
                    "Customer order",
                    order.Id.ToString(),
                    _currentUser.UserId);
            }

            // 6. Create payment
            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = total,
                PaymentMethod =
                    dto.PaymentMethodId.HasValue
                        ? "SavedCard"
                        : dto.PaymentMethod,
                Status = PaymentStatus.Paid,
                TransactionId = Guid.NewGuid().ToString("N"),
                PaidAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);

            // 7. Create checkout record
            var checkout = new Checkout
            {
                OrderId = order.Id,
                PaymentId = payment.Id,
                Status = CheckoutStatus.Completed,
                CreatedAt = DateTime.UtcNow
            };

            _db.Checkouts.Add(checkout);

            // 8. Clear cart
            _db.CartItems.RemoveRange(cart.Items);

            await _unitOfWork.SaveChangesAsync();

            // 9. Commit everything
            await transaction.CommitAsync();

            return Ok(new CheckoutResultDto
            {
                Success = true,
                TransactionId =
                    payment.TransactionId ?? string.Empty,
                Message =
                    "Order created and payment processed successfully."
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
