using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public interface IFavoriteRepository
    {
        Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId);
        Task<Favorite?> GetByUserAndProductAsync(int userId, int productId);
        Task<Favorite> AddAsync(Favorite favorite);
        Task<bool> RemoveAsync(int userId, int productId);
        Task<bool> ExistsAsync(int userId, int productId);
        Task<int> GetCountByUserIdAsync(int userId);
        Task<List<Favorite>> GetUserFavoritesAsync(int userId);
        Task<Favorite?> GetUserFavoriteAsync(int userId, int productId);
        Task<Favorite> CreateAsync(Favorite favorite);
        Task<bool> DeleteAsync(int userId, int productId);
        Task<bool> ClearUserFavoritesAsync(int userId);
    }
}
