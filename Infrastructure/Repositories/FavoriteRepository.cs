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

        public Task<List<Favorite>> GetUserFavoritesAsync(int userId) =>
            _context.Favorites
                .AsNoTracking()
                .Where(f => f.UserId == userId && f.IsActive)
                .Include(f => f.Product)
                .ThenInclude(p => p.Category)
                .OrderByDescending(f => f.CreatedAt)
                .ThenByDescending(f => f.Id)
                .ToListAsync();

        public Task<Favorite?> GetUserFavoriteAsync(int userId, int productId, bool includeInactive = false) =>
            _context.Favorites
                .Where(f => f.UserId == userId && f.ProductId == productId && (includeInactive || f.IsActive))
                .FirstOrDefaultAsync();

        public async Task AddAsync(Favorite favorite)
        {
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
        }

        public async Task ClearUserFavoritesAsync(int userId)
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
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
