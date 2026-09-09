using AdornmeStore.Application.DTOs.Offers;
using AdornmeStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/admin/offers")]
[Authorize(Roles = "Admin")]
public class OffersController : ControllerBase
{
    private readonly IOfferService _offerService;

    public OffersController(
        IOfferService offerService)
    {
        _offerService = offerService;
    }

    // GET: api/admin/offers
    [HttpGet]
    public async Task<IActionResult> GetOffers(
        [FromQuery] OfferQueryDto query)
    {
        var result =
            await _offerService.GetOffersAsync(query);

        return Ok(result);
    }

    // GET: api/admin/offers/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOffer(
        int id)
    {
        var offer =
            await _offerService.GetByIdAsync(id);

        if (offer == null)
        {
            return NotFound(new
            {
                message = "Offer not found."
            });
        }

        return Ok(offer);
    }

    // POST: api/admin/offers
    [HttpPost]
    public async Task<IActionResult> CreateOffer(
        [FromBody] CreateOfferDto dto)
    {
        var offer =
            await _offerService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetOffer),
            new { id = offer.Id },
            offer);
    }

    // PUT: api/admin/offers/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateOffer(
        int id,
        [FromBody] UpdateOfferDto dto)
    {
        var offer =
            await _offerService.UpdateAsync(id, dto);

        if (offer == null)
        {
            return NotFound(new
            {
                message = "Offer not found."
            });
        }

        return Ok(offer);
    }

    // PATCH: api/admin/offers/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] bool isActive)
    {
        var updated =
            await _offerService.UpdateStatusAsync(
                id,
                isActive);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Offer not found."
            });
        }

        return Ok(new
        {
            message = isActive
                ? "Offer activated successfully."
                : "Offer disabled successfully."
        });
    }

    // DELETE: api/admin/offers/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOffer(
        int id)
    {
        var deleted =
            await _offerService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Offer not found."
            });
        }

        return NoContent();
    }
}