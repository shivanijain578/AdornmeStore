using AdornmeStore.Application.DTOs.Products;
using AdornmeStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdornmeStore.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/products
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts(
        [FromQuery] ProductQueryDto query)
    {
        var result =
            await _productService.GetProductsAsync(query);

        return Ok(result);
    }

    // GET: api/products/5
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product =
            await _productService.GetByIdAsync(id);

        if (product == null)
            return NotFound(new
            {
                message = "Product not found."
            });

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductDto dto)
    {
        var product =
            await _productService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            product);
    }

    // PUT: api/products/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] UpdateProductDto dto)
    {
        var product =
            await _productService.UpdateAsync(id, dto);

        if (product == null)
            return NotFound(new
            {
                message = "Product not found."
            });

        return Ok(product);
    }

    // DELETE: api/products/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var deleted =
            await _productService.DeleteAsync(id);

        if (!deleted)
            return NotFound(new
            {
                message = "Product not found."
            });

        return NoContent();
    }
}