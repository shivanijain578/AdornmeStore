using AdornmeStore.Application.DTOs.Categories;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllAsync();

        Task<CategoryResponseDto> CreateAsync(
            CreateCategoryDto dto);

        Task<CategoryResponseDto?> UpdateAsync(
            int id,
            UpdateCategoryDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
