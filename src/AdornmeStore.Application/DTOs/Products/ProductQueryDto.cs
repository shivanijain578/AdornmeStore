using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Products
{
    public class ProductQueryDto
    {
        public string? Search { get; set; }

        public int? CategoryId { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public string? SortBy { get; set; }

        public bool SortDescending { get; set; } = false;

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
