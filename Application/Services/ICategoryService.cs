using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface ICategoryService
    {
        Task<BaseResponseDto<List<CategoryDto>>> GetAllCategoriesAsync();
        Task<BaseResponseDto<CategoryDto>> GetCategoryByIdAsync(int id);
        Task<BaseResponseDto<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<BaseResponseDto<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto);
        Task<BaseResponseDto<string>> DeleteCategoryAsync(int id);
    }
}
