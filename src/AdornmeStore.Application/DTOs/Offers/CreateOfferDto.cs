using AdornmeStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AdornmeStore.Application.DTOs.Offers
{
    public class CreateOfferDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public OfferType OfferType { get; set; }

        [Required]
        public OfferScope Scope { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal DiscountValue { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public List<int> ProductIds { get; set; } = new();

        public List<int> CategoryIds { get; set; } = new();
    }
}
