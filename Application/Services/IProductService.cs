using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IProductService
    {
        Task<BaseResponseDto<PagedResultDto<ProductDto>>> GetProductsAsync(ProductFilterDto filterDto);
        Task<BaseResponseDto<ProductDto>> GetProductByIdAsync(int id);
        Task<BaseResponseDto<IEnumerable<ProductDto>>> GetProductsByCategoryAsync(int categoryId);
        Task<BaseResponseDto<IEnumerable<ProductDto>>> SearchProductsAsync(string searchTerm);
        Task<BaseResponseDto<IEnumerable<ProductDto>>> GetFeaturedProductsAsync();
        Task<BaseResponseDto<IEnumerable<ProductDto>>> GetDiscountedProductsAsync();
        Task<BaseResponseDto<ProductDto>> CreateProductAsync(ProductDto productDto);
        Task<BaseResponseDto<ProductDto>> UpdateProductAsync(int id, ProductDto productDto);
        Task<BaseResponseDto<string>> DeleteProductAsync(int id);
    }
}
