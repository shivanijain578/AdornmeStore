using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Cart
{
    public class CartResponseDto
    {
        public int Id { get; set; }

        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();

        public decimal TotalAmount { get; set; }

        public int TotalItems { get; set; }
    }
}
