using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly ApplicationDbContext _context;

        public FavoriteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId && f.IsActive)
                .Include(f => f.Product)
                .ThenInclude(p => p.Category)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Favorite?> GetByUserAndProductAsync(int userId, int productId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId && f.ProductId == productId && f.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<Favorite> AddAsync(Favorite favorite)
        {
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            return favorite;
        }

        public async Task<bool> RemoveAsync(int userId, int productId)
        {
            var favorite = await _context.Favorites
                .Where(f => f.UserId == userId && f.ProductId == productId && f.IsActive)
                .FirstOrDefaultAsync();

            if (favorite == null)
                return false;

            favorite.IsActive = false;
            favorite.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int userId, int productId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.ProductId == productId && f.IsActive);
        }

        public async Task<int> GetCountByUserIdAsync(int userId)
        {
            return await _context.Favorites
                .CountAsync(f => f.UserId == userId && f.IsActive);
        }

        public async Task<List<Favorite>> GetUserFavoritesAsync(int userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId && f.IsActive)
                .Include(f => f.Product)
                .ThenInclude(p => p.Category)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<Favorite?> GetUserFavoriteAsync(int userId, int productId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId && f.ProductId == productId && f.IsActive)
                .Include(f => f.Product)
                .FirstOrDefaultAsync();
        }

        public async Task<Favorite> CreateAsync(Favorite favorite)
        {
            favorite.CreatedAt = DateTime.UtcNow;
            favorite.UpdatedAt = DateTime.UtcNow;
            
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            return favorite;
        }

        public async Task<bool> DeleteAsync(int userId, int productId)
        {
            var favorite = await _context.Favorites
                .Where(f => f.UserId == userId && f.ProductId == productId && f.IsActive)
                .FirstOrDefaultAsync();

            if (favorite == null)
                return false;

            favorite.IsActive = false;
            favorite.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearUserFavoritesAsync(int userId)
        {
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId && f.IsActive)
                .ToListAsync();

            foreach (var favorite in favorites)
            {
                favorite.IsActive = false;
                favorite.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
