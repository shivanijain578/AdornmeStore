using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Offers
{
    public class OfferQueryDto
    {
        public string? Search { get; set; }

        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
