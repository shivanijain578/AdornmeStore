using AdornmeStore.Application.DTOs.Products;
using AdornmeStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/productsPricing")]
public class ProductPricingController : ControllerBase
{
    private readonly IProductPricingService _pricingService;

    public ProductPricingController(
        IProductPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    [HttpGet("{productId:int}/pricing")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductPricingDto>> GetPricing(
        int productId,
        CancellationToken cancellationToken)
    {
        try
        {
            var pricing =
                await _pricingService.GetProductPricingAsync(
                    productId,
                    cancellationToken);

            return Ok(pricing);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }
}