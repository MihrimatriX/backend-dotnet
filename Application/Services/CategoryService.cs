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
            try
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CategoryName)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        CategoryName = c.CategoryName,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                return BaseResponseDto<List<CategoryDto>>.SuccessResult("Categories retrieved successfully", categories);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<List<CategoryDto>>.ErrorResult($"Error retrieving categories: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<CategoryDto>> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Where(c => c.Id == id && c.IsActive)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        CategoryName = c.CategoryName,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    return BaseResponseDto<CategoryDto>.ErrorResult("Category not found");
                }

                return BaseResponseDto<CategoryDto>.SuccessResult("Category retrieved successfully", category);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<CategoryDto>.ErrorResult($"Error retrieving category: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            try
            {
                var category = new Category
                {
                    CategoryName = createCategoryDto.CategoryName,
                    Description = createCategoryDto.Description,
                    ImageUrl = createCategoryDto.ImageUrl,
                    IsActive = createCategoryDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt
                };

                return BaseResponseDto<CategoryDto>.SuccessResult("Category created successfully", categoryDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<CategoryDto>.ErrorResult($"Error creating category: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return BaseResponseDto<CategoryDto>.ErrorResult("Category not found");
                }

                category.CategoryName = updateCategoryDto.CategoryName;
                category.Description = updateCategoryDto.Description;
                category.ImageUrl = updateCategoryDto.ImageUrl;
                category.IsActive = updateCategoryDto.IsActive;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt
                };

                return BaseResponseDto<CategoryDto>.SuccessResult("Category updated successfully", categoryDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<CategoryDto>.ErrorResult($"Error updating category: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return BaseResponseDto<string>.ErrorResult("Category not found");
                }

                // Soft delete - set IsActive to false
                category.IsActive = false;
                category.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return BaseResponseDto<string>.SuccessResult("Category deleted successfully", "Category deleted successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error deleting category: {ex.Message}");
            }
        }
    }
}
