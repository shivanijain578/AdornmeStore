using AdornmeStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Offers
{
    public class OfferResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public OfferType OfferType { get; set; }

        public OfferScope Scope { get; set; }

        public decimal DiscountValue { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

        public bool IsCurrentlyActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<int> ProductIds { get; set; } = new();

        public List<int> CategoryIds { get; set; } = new();
    }
}
