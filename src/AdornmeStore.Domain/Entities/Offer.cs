using AdornmeStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Domain.Entities
{
    public class Offer
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public OfferType OfferType { get; set; }

        public OfferScope Scope { get; set; }

        public decimal DiscountValue { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public ICollection<OfferProduct> OfferProducts { get; set; }
            = new List<OfferProduct>();

        public ICollection<OfferCategory> OfferCategories { get; set; }
            = new List<OfferCategory>();
    }
}
