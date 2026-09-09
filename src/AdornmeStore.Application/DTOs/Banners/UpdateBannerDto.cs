using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Banners
{
    public class UpdateBannerDto
    {
        public string Title { get; set; } = string.Empty;

        public IFormFile? Image { get; set; }

        public string? LinkUrl { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }
    }
}
