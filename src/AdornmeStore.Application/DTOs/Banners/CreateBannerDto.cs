using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Banners
{
    public class CreateBannerDto
    {
        public string Title { get; set; } = string.Empty;

        public IFormFile Image { get; set; } = null!;

        public string? LinkUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }
    }
}
