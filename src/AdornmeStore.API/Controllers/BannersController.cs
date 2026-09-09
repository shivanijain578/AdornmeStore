using AdornmeStore.Application.DTOs.Banners;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/banners")]
public class BannersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _fileStorageService;

    public BannersController(
        ApplicationDbContext db,
        IFileStorageService fileStorageService)
    {
        _db = db;
        _fileStorageService = fileStorageService;
    }

    // =========================
    // PUBLIC
    // =========================

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveBanners(
        CancellationToken cancellationToken)
    {
        var items = await _db.Set<Banner>()
            .AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ThenByDescending(b => b.CreatedAt)
            .Select(b => new BannerResponseDto
            {
                Id = b.Id,
                Title = b.Title,
                ImageUrl = b.ImageUrl,
                LinkUrl = b.LinkUrl,
                IsActive = b.IsActive,
                DisplayOrder = b.DisplayOrder,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    // =========================
    // ADMIN - GET ALL
    // =========================

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllBanners(
        CancellationToken cancellationToken)
    {
        var banners = await _db.Banners
            .AsNoTracking()
            .OrderBy(b => b.DisplayOrder)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(banners);
    }

    // =========================
    // ADMIN - CREATE
    // =========================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
      [FromForm] CreateBannerDto dto,
      CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new
            {
                message = "Banner title is required."
            });

        if (dto.Image == null || dto.Image.Length == 0)
            return BadRequest(new
            {
                message = "Banner image is required."
            });

        if (dto.DisplayOrder < 0)
            return BadRequest(new
            {
                message = "Display order cannot be negative."
            });

        try
        {
            var imageUrl =
                await _fileStorageService.SaveFileAsync(
                    dto.Image,
                    "banner-images",
                    cancellationToken);

            var banner = new Banner
            {
                Title = dto.Title.Trim(),
                ImageUrl = imageUrl,
                LinkUrl = string.IsNullOrWhiteSpace(dto.LinkUrl)
                    ? null
                    : dto.LinkUrl.Trim(),
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };

            _db.Banners.Add(banner);

            await _db.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                message = "Banner created successfully.",
                bannerId = banner.Id,
                imageUrl = banner.ImageUrl
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // =========================
    // ADMIN - UPDATE
    // =========================

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
     int id,
     [FromForm] UpdateBannerDto dto,
     CancellationToken cancellationToken)
    {
        var banner = await _db.Banners
            .FirstOrDefaultAsync(
                b => b.Id == id,
                cancellationToken);

        if (banner == null)
            return NotFound(new
            {
                message = "Banner not found."
            });

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new
            {
                message = "Banner title is required."
            });

        if (dto.DisplayOrder < 0)
            return BadRequest(new
            {
                message = "Display order cannot be negative."
            });

        try
        {
            if (dto.Image != null && dto.Image.Length > 0)
            {
                var oldImageUrl = banner.ImageUrl;

                var newImageUrl =
                    await _fileStorageService.SaveFileAsync(
                        dto.Image,
                        "banner-images",
                        cancellationToken);

                banner.ImageUrl = newImageUrl;

                await _fileStorageService.DeleteFileAsync(
                    oldImageUrl,
                    cancellationToken);
            }

            banner.Title = dto.Title.Trim();

            banner.LinkUrl = string.IsNullOrWhiteSpace(dto.LinkUrl)
                ? null
                : dto.LinkUrl.Trim();

            banner.IsActive = dto.IsActive;

            banner.DisplayOrder = dto.DisplayOrder;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                message = "Banner updated successfully."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // =========================
    // ADMIN - ENABLE / DISABLE
    // =========================

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(
     int id,
     [FromQuery] bool isActive,
     CancellationToken cancellationToken)
    {
        var banner = await _db.Banners
            .FirstOrDefaultAsync(
                b => b.Id == id,
                cancellationToken);

        if (banner == null)
            return NotFound(new
            {
                message = "Banner not found."
            });

        banner.IsActive = isActive;

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = isActive
                ? "Banner activated successfully."
                : "Banner deactivated successfully."
        });
    }

    // =========================
    // ADMIN - DELETE
    // =========================

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
     int id,
     CancellationToken cancellationToken)
    {
        var banner = await _db.Banners
            .FirstOrDefaultAsync(
                b => b.Id == id,
                cancellationToken);

        if (banner == null)
            return NotFound(new
            {
                message = "Banner not found."
            });

        await _fileStorageService.DeleteFileAsync(
            banner.ImageUrl,
            cancellationToken);

        _db.Banners.Remove(banner);

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Banner deleted successfully."
        });
    }
}