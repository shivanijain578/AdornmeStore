using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Domain.Entities
{
    public class OfferCategory
    {
        public int OfferId { get; set; }

        public int CategoryId { get; set; }

        public Offer Offer { get; set; } = null!;

        public Category Category { get; set; } = null!;
    }
}
