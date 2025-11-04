using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services;

public class SubCategoryService : ISubCategoryService
{
    private readonly ApplicationDbContext _context;

    public SubCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SubCategoryDto>> GetAllSubCategoriesAsync()
    {
        return await _context.SubCategories
            .Include(sc => sc.Category)
            .Where(sc => sc.IsActive)
            .Select(sc => new SubCategoryDto
            {
                Id = sc.Id,
                SubCategoryName = sc.SubCategoryName,
                Description = sc.Description,
                ImageUrl = sc.ImageUrl,
                CategoryId = sc.CategoryId,
                CategoryName = sc.Category.CategoryName,
                IsActive = sc.IsActive,
                CreatedAt = sc.CreatedAt,
                UpdatedAt = sc.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(int categoryId)
    {
        return await _context.SubCategories
            .Include(sc => sc.Category)
            .Where(sc => sc.CategoryId == categoryId && sc.IsActive)
            .Select(sc => new SubCategoryDto
            {
                Id = sc.Id,
                SubCategoryName = sc.SubCategoryName,
                Description = sc.Description,
                ImageUrl = sc.ImageUrl,
                CategoryId = sc.CategoryId,
                CategoryName = sc.Category.CategoryName,
                IsActive = sc.IsActive,
                CreatedAt = sc.CreatedAt,
                UpdatedAt = sc.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<SubCategoryDto?> GetSubCategoryByIdAsync(int id)
    {
        var subCategory = await _context.SubCategories
            .Include(sc => sc.Category)
            .FirstOrDefaultAsync(sc => sc.Id == id && sc.IsActive);

        if (subCategory == null)
            return null;

        return new SubCategoryDto
        {
            Id = subCategory.Id,
            SubCategoryName = subCategory.SubCategoryName,
            Description = subCategory.Description,
            ImageUrl = subCategory.ImageUrl,
            CategoryId = subCategory.CategoryId,
            CategoryName = subCategory.Category.CategoryName,
            IsActive = subCategory.IsActive,
            CreatedAt = subCategory.CreatedAt,
            UpdatedAt = subCategory.UpdatedAt
        };
    }

    public async Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto createSubCategoryDto)
    {
        var subCategory = new SubCategory
        {
            SubCategoryName = createSubCategoryDto.SubCategoryName,
            Description = createSubCategoryDto.Description,
            ImageUrl = createSubCategoryDto.ImageUrl,
            CategoryId = createSubCategoryDto.CategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SubCategories.Add(subCategory);
        await _context.SaveChangesAsync();

        return await GetSubCategoryByIdAsync(subCategory.Id) ?? throw new InvalidOperationException("Failed to create subcategory");
    }

    public async Task<SubCategoryDto> UpdateSubCategoryAsync(UpdateSubCategoryDto updateSubCategoryDto)
    {
        var subCategory = await _context.SubCategories
            .FirstOrDefaultAsync(sc => sc.Id == updateSubCategoryDto.Id);

        if (subCategory == null)
            throw new ArgumentException("SubCategory not found");

        subCategory.SubCategoryName = updateSubCategoryDto.SubCategoryName;
        subCategory.Description = updateSubCategoryDto.Description;
        subCategory.ImageUrl = updateSubCategoryDto.ImageUrl;
        subCategory.CategoryId = updateSubCategoryDto.CategoryId;
        subCategory.IsActive = updateSubCategoryDto.IsActive;
        subCategory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetSubCategoryByIdAsync(subCategory.Id) ?? throw new InvalidOperationException("Failed to update subcategory");
    }

    public async Task<bool> DeleteSubCategoryAsync(int id)
    {
        var subCategory = await _context.SubCategories
            .FirstOrDefaultAsync(sc => sc.Id == id);

        if (subCategory == null)
            return false;

        subCategory.IsActive = false;
        subCategory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
