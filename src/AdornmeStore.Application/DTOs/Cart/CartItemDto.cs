using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }

        public string? ImageUrl { get; set; }

        public int AvailableStock { get; set; }
    }
}
