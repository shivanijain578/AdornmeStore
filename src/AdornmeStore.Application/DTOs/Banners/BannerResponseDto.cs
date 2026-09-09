using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Banners
{
    public class BannerResponseDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public string? LinkUrl { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
