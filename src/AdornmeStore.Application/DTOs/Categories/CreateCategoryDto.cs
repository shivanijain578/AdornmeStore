using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Categories
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
