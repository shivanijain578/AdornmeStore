using AdornmeStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdornmeStore.API.Controllers
{
    [ApiController]
    [Route("api/wishlist")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(
            IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var wishlist =
                await _wishlistService.GetAsync();

            return Ok(wishlist);
        }

        [HttpPost("{productId:int}")]
        public async Task<IActionResult> AddToWishlist(
            int productId)
        {
            await _wishlistService.AddAsync(productId);

            return Ok(new
            {
                message = "Product added to wishlist."
            });
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> RemoveFromWishlist(
            int productId)
        {
            var removed =
                await _wishlistService.RemoveAsync(productId);

            if (!removed)
            {
                return NotFound(new
                {
                    message = "Product is not in wishlist."
                });
            }

            return Ok(new
            {
                message = "Product removed from wishlist."
            });
        }

        [HttpGet("exists/{productId:int}")]
        public async Task<IActionResult> Exists(
            int productId)
        {
            var exists =
                await _wishlistService.ExistsAsync(productId);

            return Ok(new
            {
                exists
            });
        }
    }
}
