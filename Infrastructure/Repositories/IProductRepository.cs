using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchAsync(string searchTerm);
        Task<IEnumerable<Product>> GetFeaturedAsync();
        Task<IEnumerable<Product>> GetDiscountedAsync();
        Task<Product> CreateAsync(Product product);
        Task<Product?> UpdateAsync(int id, Product product);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Product>> GetWithFiltersAsync(int? categoryId, decimal? minPrice, decimal? maxPrice, string? searchTerm, int page, int pageSize, string sortBy, string sortOrder);
        Task<int> GetTotalCountAsync(int? categoryId, decimal? minPrice, decimal? maxPrice, string? searchTerm);
    }
}
