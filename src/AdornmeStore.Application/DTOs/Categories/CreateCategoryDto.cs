using Microsoft.AspNetCore.Http;

namespace AdornmeStore.Application.DTOs.Categories
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public IFormFile? Image { get; set; }

        public bool IsVisible { get; set; } = true;
    }
}