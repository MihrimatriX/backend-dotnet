using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Where(c => c.IsActive && c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Category> CreateAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category?> UpdateAsync(int id, Category category)
        {
            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null) return null;

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.Description = category.Description;
            existingCategory.ImageUrl = category.ImageUrl;
            existingCategory.IsActive = category.IsActive;
            existingCategory.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingCategory;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Categories
                .AnyAsync(c => c.CategoryName == name);
        }
    }
}
