using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Domain.Entities
{
    public class OfferProduct
    {
        public int OfferId { get; set; }

        public int ProductId { get; set; }

        public Offer Offer { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}
