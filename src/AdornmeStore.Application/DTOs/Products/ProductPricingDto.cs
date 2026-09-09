using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Products
{
    public class ProductPricingDto
    {
        public decimal OriginalPrice { get; set; }

        public decimal SellingPrice { get; set; }

        public decimal DiscountPercentage { get; set; }

        public decimal OfferDiscount { get; set; }

        public decimal FinalPrice { get; set; }

        public int? OfferId { get; set; }

        public string? OfferName { get; set; }
    }
}
