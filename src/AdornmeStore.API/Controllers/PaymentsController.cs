using AdornmeStore.API.Models;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly ApplicationDbContext _db;

    public PaymentsController(ICurrentUserService currentUser, ApplicationDbContext db)
    {
        _currentUser = currentUser;
        _db = db;
    }

    [HttpGet("methods")]
    public async Task<IActionResult> GetMethods()
    {
        var methods = await _db.PaymentMethods
            .AsNoTracking()
            .Where(m => m.UserId == _currentUser.UserId)
            .Select(m => new PaymentMethodDto
            {
                Id = m.Id,
                CardHolderName = m.CardHolderName,
                CardLast4 = m.CardLast4,
                CardType = m.CardType,
                ExpiryMonth = m.ExpiryMonth,
                ExpiryYear = m.ExpiryYear,
                IsDefault = m.IsDefault
            })
            .ToListAsync();

        return Ok(methods);
    }

    [HttpPost("methods")]
    public async Task<IActionResult> AddMethod([FromBody] CreatePaymentMethodDto dto)
    {
        var method = new PaymentMethod
        {
            UserId = _currentUser.UserId,
            CardHolderName = dto.CardHolderName,
            CardLast4 = dto.CardLast4,
            CardType = dto.CardType,
            ExpiryMonth = dto.ExpiryMonth,
            ExpiryYear = dto.ExpiryYear,
            IsDefault = dto.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        _db.PaymentMethods.Add(method);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMethods), new { id = method.Id }, new PaymentMethodDto
        {
            Id = method.Id,
            CardHolderName = method.CardHolderName,
            CardLast4 = method.CardLast4,
            CardType = method.CardType,
            ExpiryMonth = method.ExpiryMonth,
            ExpiryYear = method.ExpiryYear,
            IsDefault = method.IsDefault
        });
    }

    [HttpDelete("methods/{id:int}")]
    public async Task<IActionResult> DeleteMethod(int id)
    {
        var method = await _db.PaymentMethods.FirstOrDefaultAsync(m => m.Id == id && m.UserId == _currentUser.UserId);

        if (method == null)
            return NotFound(new { message = "Payment method not found." });

        _db.PaymentMethods.Remove(method);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
