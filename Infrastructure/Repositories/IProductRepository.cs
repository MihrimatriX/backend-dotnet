using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Repositories
{
    /// <summary>Ürün liste sorgusu; <see cref="SortBy"/> bilinmiyorsa Id'ye göre sıralanır.</summary>
    public sealed record ProductQuery(
        int? CategoryId,
        int? SubCategoryId,
        decimal? MinPrice,
        decimal? MaxPrice,
        string? SearchTerm,
        string? SortBy,
        bool Descending,
        int Skip,
        int Take);

    /// <summary>Aktif yorumlardan hesaplanan puan özeti.</summary>
    public sealed record ProductRatingSummary(double AverageRating, int TotalReviews);

    public interface IProductRepository
    {
        /// <summary>Aktif ürün (kategori ve alt kategori yüklü).</summary>
        Task<Product?> GetActiveByIdAsync(int id);

        /// <summary>Aktif/pasif fark etmeksizin ürün (yönetici güncellemesi için).</summary>
        Task<Product?> GetByIdAsync(int id);

        Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId);
        Task<IReadOnlyList<Product>> SearchAsync(string searchTerm);
        Task<IReadOnlyList<Product>> GetFeaturedAsync(DateTime createdSince, int take);
        Task<IReadOnlyList<Product>> GetDiscountedAsync(int take);
        Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPageAsync(ProductQuery query);

        /// <summary>Verilen ürünlerin puan özetleri tek sorguda (N+1 yok); yorumu olmayan ürün sözlükte yer almaz.</summary>
        Task<IReadOnlyDictionary<int, ProductRatingSummary>> GetRatingSummariesAsync(IReadOnlyCollection<int> productIds);

        Task<Product> CreateAsync(Product product);
        Task SaveChangesAsync();
    }
}
