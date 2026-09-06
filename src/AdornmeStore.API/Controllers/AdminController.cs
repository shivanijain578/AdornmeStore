using AdornmeStore.API.Models;
using AdornmeStore.Domain.Enums;
using AdornmeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var ordersCount = await _db.Orders.CountAsync();
        var usersCount = await _db.Users.CountAsync();
        var paymentsCount = await _db.Payments.CountAsync();
        var addressesCount = await _db.Addresses.CountAsync();
        var totalRevenue = await _db.Payments.SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var dto = new AdminSummaryDto
        {
            OrdersCount = ordersCount,
            UsersCount = usersCount,
            PaymentsCount = paymentsCount,
            BannersCount = 0, // banners not implemented yet
            TotalRevenue = totalRevenue
            ,
            ReceivedOrders = await _db.Orders.CountAsync(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Returned)
            ,
            PendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending)
            ,
            ShippedOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Shipped)
            ,
            DeliveredOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Delivered)
            ,
            ReturnedOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Returned)
            ,
            CancelledOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled)
        };

        return Ok(dto);
    }

    [HttpGet("addresses")]
    public async Task<IActionResult> GetAddresses()
    {
        var addresses = await _db.Addresses
            .Include(a => a.User)
            .Select(a => new AddressDto
            {
                Id = a.Id,
                UserId = a.UserId,
                FullName = a.FullName,
                PhoneNumber = a.PhoneNumber,
                AddressLine1 = a.AddressLine1,
                AddressLine2 = a.AddressLine2,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                IsDefault = a.IsDefault,
                UserEmail = a.User.Email
            })
            .ToListAsync();

        return Ok(addresses);
    }
}
