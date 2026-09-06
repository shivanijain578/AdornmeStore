using AdornmeStore.API.Models;
using AdornmeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/banners")]
public class BannersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BannersController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await _db.Set<AdornmeStore.Domain.Entities.Banner>()
            .AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .Select(b => new BannerDto
            {
                Id = b.Id,
                Title = b.Title,
                ImageUrl = b.ImageUrl,
                LinkUrl = b.LinkUrl
            })
            .ToListAsync();

        return Ok(items);
    }
}
