using EcommerceBackend.Application.Common;
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

    /// <summary>Aktif alt kategoriler: kategori adı, sonra alt kategori adına göre.</summary>
    public async Task<BaseResponseDto<List<SubCategoryDto>>> GetAllSubCategoriesAsync()
    {
        var subCategories = await _context.SubCategories
            .AsNoTracking()
            .Include(sc => sc.Category)
            .Where(sc => sc.IsActive)
            .OrderBy(sc => sc.Category.CategoryName)
            .ThenBy(sc => sc.SubCategoryName)
            .ToListAsync();

        return BaseResponseDto<List<SubCategoryDto>>.SuccessResult(
            "SubCategories retrieved successfully",
            subCategories.Select(ToDto).ToList());
    }

    public async Task<BaseResponseDto<List<SubCategoryDto>>> GetSubCategoriesByCategoryIdAsync(int categoryId)
    {
        var subCategories = await _context.SubCategories
            .AsNoTracking()
            .Include(sc => sc.Category)
            .Where(sc => sc.CategoryId == categoryId && sc.IsActive)
            .OrderBy(sc => sc.SubCategoryName)
            .ToListAsync();

        return BaseResponseDto<List<SubCategoryDto>>.SuccessResult(
            "SubCategories retrieved successfully",
            subCategories.Select(ToDto).ToList());
    }

    public async Task<BaseResponseDto<SubCategoryDto>> GetSubCategoryByIdAsync(int id)
    {
        var subCategory = await _context.SubCategories
            .AsNoTracking()
            .Include(sc => sc.Category)
            .FirstOrDefaultAsync(sc => sc.Id == id && sc.IsActive);

        return subCategory == null
            ? SubCategoryNotFound<SubCategoryDto>()
            : BaseResponseDto<SubCategoryDto>.SuccessResult("SubCategory retrieved successfully", ToDto(subCategory));
    }

    public async Task<BaseResponseDto<SubCategoryDto>> CreateSubCategoryAsync(CreateSubCategoryDto createSubCategoryDto)
    {
        var category = await FindActiveCategoryAsync(createSubCategoryDto.CategoryId);
        if (category == null)
            return CategoryNotFound();

        var name = createSubCategoryDto.SubCategoryName.Trim();
        if (await NameExistsAsync(name, category.Id, exceptId: null))
            return SubCategoryExists();

        var subCategory = new SubCategory
        {
            SubCategoryName = name,
            Description = createSubCategoryDto.Description,
            ImageUrl = createSubCategoryDto.ImageUrl,
            CategoryId = category.Id,
            Category = category,
            IsActive = createSubCategoryDto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SubCategories.Add(subCategory);
        await _context.SaveChangesAsync();

        return BaseResponseDto<SubCategoryDto>.SuccessResult("SubCategory created successfully", ToDto(subCategory));
    }

    /// <summary>Pasife alma dahil güncelleme; <c>isActive</c> gönderilmezse değişmez.</summary>
    public async Task<BaseResponseDto<SubCategoryDto>> UpdateSubCategoryAsync(int id, UpdateSubCategoryDto updateSubCategoryDto)
    {
        if (updateSubCategoryDto.Id is { } bodyId && bodyId != id)
            return BaseResponseDto<SubCategoryDto>.Fail("ID mismatch", ErrorCodes.IdMismatch);

        var subCategory = await _context.SubCategories.FirstOrDefaultAsync(sc => sc.Id == id);
        if (subCategory == null)
            return SubCategoryNotFound<SubCategoryDto>();

        var category = await FindActiveCategoryAsync(updateSubCategoryDto.CategoryId);
        if (category == null)
            return CategoryNotFound();

        var name = updateSubCategoryDto.SubCategoryName.Trim();
        if (await NameExistsAsync(name, category.Id, exceptId: id))
            return SubCategoryExists();

        subCategory.SubCategoryName = name;
        subCategory.Description = updateSubCategoryDto.Description;
        subCategory.ImageUrl = updateSubCategoryDto.ImageUrl;
        subCategory.CategoryId = category.Id;
        subCategory.Category = category;
        if (updateSubCategoryDto.IsActive is { } isActive)
            subCategory.IsActive = isActive;
        subCategory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return BaseResponseDto<SubCategoryDto>.SuccessResult("SubCategory updated successfully", ToDto(subCategory));
    }

    public async Task<BaseResponseDto<bool>> DeleteSubCategoryAsync(int id)
    {
        var subCategory = await _context.SubCategories.FirstOrDefaultAsync(sc => sc.Id == id && sc.IsActive);
        if (subCategory == null)
            return SubCategoryNotFound<bool>();

        subCategory.IsActive = false;
        subCategory.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return BaseResponseDto<bool>.SuccessResult("SubCategory deleted successfully", true);
    }

    private Task<Category?> FindActiveCategoryAsync(int categoryId) =>
        _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && c.IsActive);

    /// <summary>(ad, kategori) benzersizdir (veritabanı indeksi); kontrol harf duyarsızdır.</summary>
    private Task<bool> NameExistsAsync(string name, int categoryId, int? exceptId)
    {
        var lowered = name.ToLower();
        return _context.SubCategories.AnyAsync(sc =>
            sc.CategoryId == categoryId
            && sc.SubCategoryName.ToLower() == lowered
            && (exceptId == null || sc.Id != exceptId));
    }

    private static BaseResponseDto<T> SubCategoryNotFound<T>() =>
        BaseResponseDto<T>.NotFound("SubCategory not found", ErrorCodes.SubCategoryNotFound);

    private static BaseResponseDto<SubCategoryDto> CategoryNotFound() =>
        BaseResponseDto<SubCategoryDto>.Fail("Category not found", ErrorCodes.CategoryNotFound);

    private static BaseResponseDto<SubCategoryDto> SubCategoryExists() =>
        BaseResponseDto<SubCategoryDto>.Fail("SubCategory name already exists in this category", ErrorCodes.SubCategoryExists);

    private static SubCategoryDto ToDto(SubCategory sc) => new()
    {
        Id = sc.Id,
        SubCategoryName = sc.SubCategoryName,
        Description = sc.Description,
        ImageUrl = sc.ImageUrl,
        CategoryId = sc.CategoryId,
        CategoryName = sc.Category?.CategoryName ?? string.Empty,
        IsActive = sc.IsActive,
        CreatedAt = sc.CreatedAt,
        UpdatedAt = sc.UpdatedAt
    };
}
