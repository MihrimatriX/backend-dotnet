using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponseDto<List<CategoryDto>>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return BaseResponseDto<List<CategoryDto>>.SuccessResult(
                "Categories retrieved successfully",
                categories.Select(ToDto).ToList());
        }

        public async Task<BaseResponseDto<CategoryDto>> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            return category == null
                ? CategoryNotFound()
                : BaseResponseDto<CategoryDto>.SuccessResult("Category retrieved successfully", ToDto(category));
        }

        public async Task<BaseResponseDto<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var name = createCategoryDto.CategoryName.Trim();
            if (await NameExistsAsync(name, exceptId: null))
                return CategoryExists();

            var category = new Category
            {
                CategoryName = name,
                Description = createCategoryDto.Description,
                ImageUrl = createCategoryDto.ImageUrl,
                IsActive = createCategoryDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return BaseResponseDto<CategoryDto>.SuccessResult("Category created successfully", ToDto(category));
        }

        /// <summary>Pasif kategoriler de güncellenebilir; <c>isActive</c> gönderilmezse değişmez.</summary>
        public async Task<BaseResponseDto<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return CategoryNotFound();

            var name = updateCategoryDto.CategoryName.Trim();
            if (await NameExistsAsync(name, exceptId: id))
                return CategoryExists();

            category.CategoryName = name;
            category.Description = updateCategoryDto.Description;
            category.ImageUrl = updateCategoryDto.ImageUrl;
            if (updateCategoryDto.IsActive is { } isActive)
                category.IsActive = isActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BaseResponseDto<CategoryDto>.SuccessResult("Category updated successfully", ToDto(category));
        }

        public async Task<BaseResponseDto<string>> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
            if (category == null)
                return BaseResponseDto<string>.NotFound("Category not found", ErrorCodes.CategoryNotFound);

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<string>.SuccessResult("Category deleted successfully", "Category deleted successfully");
        }

        /// <summary>Ad benzersizliği harf duyarsızdır ve pasif kategorileri de kapsar (veritabanındaki benzersiz indeks).</summary>
        private Task<bool> NameExistsAsync(string name, int? exceptId)
        {
            var lowered = name.ToLower();
            return _context.Categories.AnyAsync(c =>
                c.CategoryName.ToLower() == lowered && (exceptId == null || c.Id != exceptId));
        }

        private static BaseResponseDto<CategoryDto> CategoryNotFound() =>
            BaseResponseDto<CategoryDto>.NotFound("Category not found", ErrorCodes.CategoryNotFound);

        private static BaseResponseDto<CategoryDto> CategoryExists() =>
            BaseResponseDto<CategoryDto>.Fail("Category name already exists", ErrorCodes.CategoryExists);

        private static CategoryDto ToDto(Category c) => new()
        {
            Id = c.Id,
            CategoryName = c.CategoryName,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
}
