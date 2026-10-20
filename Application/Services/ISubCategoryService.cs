using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services;

public interface ISubCategoryService
{
    Task<BaseResponseDto<List<SubCategoryDto>>> GetAllSubCategoriesAsync();
    Task<BaseResponseDto<List<SubCategoryDto>>> GetSubCategoriesByCategoryIdAsync(int categoryId);
    Task<BaseResponseDto<SubCategoryDto>> GetSubCategoryByIdAsync(int id);
    Task<BaseResponseDto<SubCategoryDto>> CreateSubCategoryAsync(CreateSubCategoryDto createSubCategoryDto);
    Task<BaseResponseDto<SubCategoryDto>> UpdateSubCategoryAsync(int id, UpdateSubCategoryDto updateSubCategoryDto);
    Task<BaseResponseDto<bool>> DeleteSubCategoryAsync(int id);
}
