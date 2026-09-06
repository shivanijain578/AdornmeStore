using AdornmeStore.Application.DTOs.Categories;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;

namespace AdornmeStore.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(
        ICategoryRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();

        return categories.Select(MapToDto);
    }

    public async Task<CategoryResponseDto> CreateAsync(
        CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(category);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryResponseDto?> UpdateAsync(
        int id,
        UpdateCategoryDto dto)
    {
        var category = await _repository.GetByIdAsync(id);

        if (category == null)
            return null;

        category.Name = dto.Name.Trim();
        category.Description = dto.Description?.Trim();
        category.IsActive = dto.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(category);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);

        if (category == null)
            return false;

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(category);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static CategoryResponseDto MapToDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }
}