using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _context.Reviews
                .Where(r => r.IsActive)
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Where(r => r.Id == id && r.IsActive)
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsActive)
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(int userId)
        {
            return await _context.Reviews
                .Where(r => r.UserId == userId && r.IsActive)
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetByUserAndProductAsync(int userId, int productId)
        {
            return await _context.Reviews
                .Where(r => r.UserId == userId && r.ProductId == productId && r.IsActive)
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync();
        }

        public async Task<Review> AddAsync(Review review)
        {
            review.CreatedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;
            
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<Review> UpdateAsync(Review review)
        {
            review.UpdatedAt = DateTime.UtcNow;
            
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

            if (review == null)
                return false;

            review.IsActive = false;
            review.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Reviews
                .AnyAsync(r => r.Id == id && r.IsActive);
        }

        public async Task<double> GetAverageRatingByProductIdAsync(int productId)
        {
            var averageRating = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsActive)
                .AverageAsync(r => (double?)r.Rating);

            return averageRating ?? 0.0;
        }

        public async Task<int> GetReviewCountByProductIdAsync(int productId)
        {
            return await _context.Reviews
                .CountAsync(r => r.ProductId == productId && r.IsActive);
        }

        public async Task<IEnumerable<Review>> GetVerifiedReviewsAsync()
        {
            return await _context.Reviews
                .Where(r => r.IsActive && r.IsVerified)
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
