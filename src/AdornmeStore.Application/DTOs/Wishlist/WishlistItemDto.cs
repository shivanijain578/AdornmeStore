using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Wishlist
{
    public class WishlistItemDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public int StockQuantity { get; set; }

        public DateTime AddedAt { get; set; }
    }
}
