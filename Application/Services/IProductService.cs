using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IProductService
    {
        Task<BaseResponseDto<PagedResultDto<ProductDto>>> GetProductsAsync(ProductFilterDto filterDto);
        Task<BaseResponseDto<ProductDto>> GetProductByIdAsync(int id);
        Task<BaseResponseDto<List<ProductDto>>> GetProductsByCategoryAsync(int categoryId);
        Task<BaseResponseDto<List<ProductDto>>> SearchProductsAsync(string searchTerm);
        Task<BaseResponseDto<List<ProductDto>>> GetFeaturedProductsAsync();
        Task<BaseResponseDto<List<ProductDto>>> GetDiscountedProductsAsync();
        Task<BaseResponseDto<ProductDto>> CreateProductAsync(ProductDto productDto);
        Task<BaseResponseDto<ProductDto>> UpdateProductAsync(int id, ProductDto productDto);
        Task<BaseResponseDto<string>> DeleteProductAsync(int id);
    }
}
