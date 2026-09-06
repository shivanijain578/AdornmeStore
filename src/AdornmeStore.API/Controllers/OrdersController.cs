using AdornmeStore.API.Models;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Enums;
using AdornmeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly ApplicationDbContext _db;

    public OrdersController(ICurrentUserService currentUser, ApplicationDbContext db)
    {
        _currentUser = currentUser;
        _db = db;
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
    public async Task<IActionResult> CancelOrder(int id)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == _currentUser.UserId);

        if (order == null)
            return NotFound(new { message = "Order not found." });

        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
        {
            return BadRequest(new
            {
                message = "This order can no longer be cancelled."
            });
        }

        order.Status = OrderStatus.Cancelled;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Order cancelled successfully.",
            status = order.Status.ToString()
        });
    }
}
