using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services;

public interface ISubCategoryService
{
    Task<IEnumerable<SubCategoryDto>> GetAllSubCategoriesAsync();
    Task<IEnumerable<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(int categoryId);
    Task<SubCategoryDto?> GetSubCategoryByIdAsync(int id);
    Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto createSubCategoryDto);
    Task<SubCategoryDto> UpdateSubCategoryAsync(UpdateSubCategoryDto updateSubCategoryDto);
    Task<bool> DeleteSubCategoryAsync(int id);
}
