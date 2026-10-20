using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public interface IFavoriteRepository
    {
        /// <summary>Aktif favoriler (ürün ve kategori yüklü), en yeni önce.</summary>
        Task<List<Favorite>> GetUserFavoritesAsync(int userId);

        /// <summary>Kullanıcı + ürün satırı; <paramref name="includeInactive"/> ile silinmiş (pasif) satır da döner.</summary>
        Task<Favorite?> GetUserFavoriteAsync(int userId, int productId, bool includeInactive = false);

        Task AddAsync(Favorite favorite);
        Task ClearUserFavoritesAsync(int userId);
        Task SaveChangesAsync();
    }
}
