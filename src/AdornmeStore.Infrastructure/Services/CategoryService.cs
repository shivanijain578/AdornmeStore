using AdornmeStore.Application.DTOs.Categories;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Infrastructure.Services;

namespace AdornmeStore.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    public CategoryService(
     ICategoryRepository repository,
     IUnitOfWork unitOfWork,
     IFileStorageService fileStorageService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();

        return categories.Select(MapToDto);
    }

    public async Task<CategoryResponseDto> CreateAsync(
     CreateCategoryDto dto)
    {
        if (dto.Image == null || dto.Image.Length == 0)
        {
            throw new ArgumentException(
                "Category image is required.");
        }

        var imageUrl =
            await _fileStorageService.SaveFileAsync(
                dto.Image,
                "category-images");

        var category = new Category
        {
            Name = dto.Name.Trim(),

            Description =
                string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim(),

            ImageUrl = imageUrl,

            IsVisible = dto.IsVisible,

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(category);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(category);
    }
    public async Task<CategoryResponseDto?> UpdateAsync(
        int id,
        UpdateCategoryDto dto)
    {
        var category =
            await _repository.GetByIdAsync(id);

        if (category == null)
            return null;


        category.Name =
            dto.Name.Trim();


        category.Description =
            string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();


        category.IsVisible =
            dto.IsVisible;


        /*
         * Image is optional during update.
         *
         * No new image:
         * keep existing image.
         *
         * New image:
         * delete old image and save new image.
         */
        if (dto.Image != null &&
            dto.Image.Length > 0)
        {
            var oldImageUrl =
                category.ImageUrl;


            var newImageUrl =
                await _fileStorageService.SaveFileAsync(
                    dto.Image,
                    "category-images");


            category.ImageUrl =
                newImageUrl;


            if (!string.IsNullOrWhiteSpace(
                    oldImageUrl))
            {
                await _fileStorageService
                    .DeleteFileAsync(
                        oldImageUrl);
            }
        }


        category.UpdatedAt =
            DateTime.UtcNow;


        await _repository.UpdateAsync(category);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(category);
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);

        if (category == null)
            return false;

        category.IsVisible = false;
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
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsVisible = category.IsVisible
        };
    }
}