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

    // =====================================================
    // DASHBOARD SUMMARY
    // =====================================================

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        CancellationToken cancellationToken)
    {
        var ordersCount = await _db.Orders
            .CountAsync(cancellationToken);

        var usersCount = await _db.Users
            .CountAsync(cancellationToken);

        var productsCount = await _db.Products
            .CountAsync(cancellationToken);

        var activeProductsCount = await _db.Products
            .CountAsync(p => p.IsActive, cancellationToken);

        var bannersCount = await _db.Banners
            .CountAsync(cancellationToken);

        var activeBannersCount = await _db.Banners
            .CountAsync(b => b.IsActive, cancellationToken);

        var paymentsCount = await _db.Payments
            .CountAsync(cancellationToken);

        var totalRevenue = await _db.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .SumAsync(
                p => (decimal?)p.Amount,
                cancellationToken) ?? 0m;

        var pendingOrders = await _db.Orders
            .CountAsync(
                o => o.Status == OrderStatus.Pending,
                cancellationToken);

        var confirmedOrders = await _db.Orders
            .CountAsync(
                o => o.Status == OrderStatus.Confirmed,
                cancellationToken);

        var shippedOrders = await _db.Orders
            .CountAsync(
                o => o.Status == OrderStatus.Shipped,
                cancellationToken);

        var deliveredOrders = await _db.Orders
            .CountAsync(
                o => o.Status == OrderStatus.Delivered,
                cancellationToken);

        var returnedOrders = await _db.Orders
            .CountAsync(
                o => o.Status == OrderStatus.Returned,
                cancellationToken);

        var cancelledOrders = await _db.Orders
            .CountAsync(
                o => o.Status == OrderStatus.Cancelled,
                cancellationToken);

        var lowStockProducts = await _db.Products
            .CountAsync(
                p => p.StockQuantity > 0 &&
                     p.StockQuantity <= p.LowStockThreshold,
                cancellationToken);

        var outOfStockProducts = await _db.Products
            .CountAsync(
                p => p.StockQuantity <= 0,
                cancellationToken);

        return Ok(new
        {
            ordersCount,
            usersCount,
            productsCount,
            activeProductsCount,
            bannersCount,
            activeBannersCount,
            paymentsCount,
            totalRevenue,

            orderStatus = new
            {
                pending = pendingOrders,
                confirmed = confirmedOrders,
                shipped = shippedOrders,
                delivered = deliveredOrders,
                returned = returnedOrders,
                cancelled = cancelledOrders
            },

            inventory = new
            {
                lowStockProducts,
                outOfStockProducts
            }
        });
    }


    // =====================================================
    // REVENUE ANALYTICS
    // =====================================================

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (days < 1)
            days = 30;

        if (days > 365)
            days = 365;

        var startDate = DateTime.UtcNow.Date.AddDays(-(days - 1));

        var revenue = await _db.Payments
            .Where(p =>
                p.Status == PaymentStatus.Paid &&
                p.PaidAt.HasValue &&
                p.PaidAt.Value >= startDate)
            .GroupBy(p => p.PaidAt!.Value.Date)
            .Select(g => new
            {
                date = g.Key,
                revenue = g.Sum(x => x.Amount),
                payments = g.Count()
            })
            .OrderBy(x => x.date)
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            days,
            startDate,
            endDate = DateTime.UtcNow.Date,
            data = revenue
        });
    }


    // =====================================================
    // ORDER STATUS ANALYTICS
    // =====================================================

    [HttpGet("order-status")]
    public async Task<IActionResult> GetOrderStatus(
        CancellationToken cancellationToken)
    {
        var data = await _db.Orders
            .GroupBy(o => o.Status)
            .Select(g => new
            {
                status = g.Key.ToString(),
                count = g.Count()
            })
            .OrderBy(x => x.status)
            .ToListAsync(cancellationToken);

        return Ok(data);
    }


    // =====================================================
    // TOP SELLING PRODUCTS
    // =====================================================

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit < 1)
            limit = 10;

        if (limit > 50)
            limit = 50;

        var products = await _db.OrderItems
            .Where(item =>
                item.Order.Status != OrderStatus.Cancelled &&
                item.Order.Status != OrderStatus.Returned)
            .GroupBy(item => new
            {
                item.ProductId,
                item.ProductName
            })
            .Select(g => new
            {
                productId = g.Key.ProductId,
                productName = g.Key.ProductName,
                quantitySold = g.Sum(x => x.Quantity),
                revenue = g.Sum(
                    x => x.UnitPrice * x.Quantity)
            })
            .OrderByDescending(x => x.quantitySold)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return Ok(products);
    }


    // =====================================================
    // LOW STOCK PRODUCTS
    // =====================================================

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts(
        CancellationToken cancellationToken)
    {
        var products = await _db.Products
            .AsNoTracking()
            .Where(p =>
                p.StockQuantity <= p.LowStockThreshold)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new
            {
                productId = p.Id,
                productName = p.Name,
                stockQuantity = p.StockQuantity,
                lowStockThreshold = p.LowStockThreshold,
                isOutOfStock = p.StockQuantity <= 0,
                isActive = p.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(products);
    }


    // =====================================================
    // ADMIN - ADDRESSES
    // =====================================================

    [HttpGet("addresses")]
    public async Task<IActionResult> GetAddresses(
        CancellationToken cancellationToken)
    {
        var addresses = await _db.Addresses
            .AsNoTracking()
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
            .ToListAsync(cancellationToken);

        return Ok(addresses);
    }
}