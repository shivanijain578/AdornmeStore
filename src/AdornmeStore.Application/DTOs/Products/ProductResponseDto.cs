using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Products
{
    public class ProductResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal OriginalPrice { get; set; }

        public decimal SellingPrice { get; set; }

        public decimal DiscountPercentage { get; set; }

        public int StockQuantity { get; set; }

        public string? Material { get; set; }

        public int Gender { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<ProductImageResponseDto> Images { get; set; } = new();

    }

    public class ProductImageResponseDto
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}
