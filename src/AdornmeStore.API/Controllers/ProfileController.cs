using AdornmeStore.API.Models;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly ApplicationDbContext _db;

    public ProfileController(
        ICurrentUserService currentUser,
        ApplicationDbContext db)
    {
        _currentUser = currentUser;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        if (!_currentUser.IsAuthenticated)
            return Unauthorized();

        var user = await _db.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId);

        if (user == null)
            return NotFound(new { message = "User not found." });

        var dto = new ProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Addresses = user.Addresses.Select(a => new AddressDto
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
                UserEmail = user.Email
            }).ToList()
        };

        return Ok(dto);
    }

    [HttpPost("addresses")]
    public async Task<IActionResult> AddAddress([FromBody] CreateAddressDto dto)
    {
        if (!_currentUser.IsAuthenticated)
            return Unauthorized();

        var address = new AdornmeStore.Domain.Entities.Address
        {
            UserId = _currentUser.UserId,
            FullName = dto.FullName.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            AddressLine1 = dto.AddressLine1.Trim(),
            AddressLine2 = string.IsNullOrWhiteSpace(dto.AddressLine2) ? null : dto.AddressLine2.Trim(),
            City = dto.City.Trim(),
            State = dto.State.Trim(),
            PostalCode = dto.PostalCode.Trim(),
            IsDefault = dto.IsDefault
        };

        if (address.IsDefault)
        {
            var existingAddresses = await _db.Addresses
                .Where(item => item.UserId == _currentUser.UserId)
                .ToListAsync();

            foreach (var existingAddress in existingAddresses)
            {
                existingAddress.IsDefault = false;
            }
        }

        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();

        return Ok(new AddressDto
        {
            Id = address.Id,
            UserId = address.UserId,
            FullName = address.FullName,
            PhoneNumber = address.PhoneNumber,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            IsDefault = address.IsDefault,
            UserEmail = string.Empty
        });
    }
}
