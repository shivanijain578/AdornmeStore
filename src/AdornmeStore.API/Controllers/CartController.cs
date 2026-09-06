using AdornmeStore.Application.DTOs.Cart;
using AdornmeStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdornmeStore.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(
            ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart =
                await _cartService.GetCartAsync();

            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem(
            [FromBody] AddCartItemDto dto)
        {
            var cart =
                await _cartService.AddItemAsync(dto);

            return Ok(cart);
        }

        [HttpPut("items/{productId:int}")]
        public async Task<IActionResult> UpdateItem(
            int productId,
            [FromBody] UpdateCartItemDto dto)
        {
            var cart =
                await _cartService.UpdateItemAsync(
                    productId,
                    dto);

            if (cart == null)
            {
                return NotFound(new
                {
                    message = "Cart item not found."
                });
            }

            return Ok(cart);
        }

        [HttpDelete("items/{productId:int}")]
        public async Task<IActionResult> RemoveItem(
            int productId)
        {
            var removed =
                await _cartService.RemoveItemAsync(
                    productId);

            if (!removed)
            {
                return NotFound(new
                {
                    message = "Cart item not found."
                });
            }

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearAsync();

            return NoContent();
        }
    }
}
