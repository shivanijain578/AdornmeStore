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

    public CheckoutController(
        ICurrentUserService currentUser,
        ApplicationDbContext db,
        IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _db = db;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto dto)
    {
        if (!_currentUser.IsAuthenticated)
            return Unauthorized();

        // load user's cart with product details
        var cart = await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId);

        if (cart == null || cart.Items == null || !cart.Items.Any())
            return BadRequest(new { message = "Cart is empty." });

        // validate address
        var address = await _db.Addresses
            .FirstOrDefaultAsync(a => a.Id == dto.AddressId && a.UserId == _currentUser.UserId);

        if (address == null)
            return BadRequest(new { message = "Invalid address." });

        // calculate total
        var total = cart.Items.Sum(i => i.Product.Price * i.Quantity);

        // create order
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
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Quantity = item.Quantity,
                UnitPrice = item.Product.Price
            });
        }

        _db.Orders.Add(order);
        await _unitOfWork.SaveChangesAsync();

        // create payment
        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = total,
            PaymentMethod = dto.PaymentMethodId.HasValue ? "SavedCard" : dto.PaymentMethod,
            Status = PaymentStatus.Paid,
            TransactionId = Guid.NewGuid().ToString("N"),
            PaidAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _unitOfWork.SaveChangesAsync();

        // associate payment with order (optional, navigation handled by FK)
        order.Payment = payment;
        await _unitOfWork.SaveChangesAsync();

        // create checkout record
        var checkout = new Checkout
        {
            OrderId = order.Id,
            PaymentId = payment.Id,
            Status = CheckoutStatus.Completed,
            CreatedAt = DateTime.UtcNow
        };

        _db.Checkouts.Add(checkout);
        await _unitOfWork.SaveChangesAsync();

        // clear cart
        foreach (var item in cart.Items.ToList())
        {
            _db.CartItems.Remove(item);
        }

        await _unitOfWork.SaveChangesAsync();

        var result = new CheckoutResultDto
        {
            Success = true,
            TransactionId = payment.TransactionId ?? string.Empty,
            Message = "Order created and payment processed."
        };

        return Ok(result);
    }
}
