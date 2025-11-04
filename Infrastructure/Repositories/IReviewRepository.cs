using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<Review?> GetByIdAsync(int id);
        Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetByUserIdAsync(int userId);
        Task<Review?> GetByUserAndProductAsync(int userId, int productId);
        Task<Review> AddAsync(Review review);
        Task<Review> UpdateAsync(Review review);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<double> GetAverageRatingByProductIdAsync(int productId);
        Task<int> GetReviewCountByProductIdAsync(int productId);
        Task<IEnumerable<Review>> GetVerifiedReviewsAsync();
    }
}
