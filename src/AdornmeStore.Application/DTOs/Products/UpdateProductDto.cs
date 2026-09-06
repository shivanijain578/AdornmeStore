using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Products
{
    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        public bool IsActive { get; set; }
    }
}
