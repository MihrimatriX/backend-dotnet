using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Where(p => p.IsActive && p.Id == id)
                .Include(p => p.Category)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.IsActive && p.CategoryId == categoryId)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _context.Products
                .Where(p => p.IsActive && 
                    (p.ProductName.ToLower().Contains(term) || 
                     (p.Description != null && p.Description.ToLower().Contains(term))))
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetFeaturedAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive && (p.Discount > 20 || p.CreatedAt > DateTime.UtcNow.AddDays(-7)))
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetDiscountedAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive && p.Discount > 0)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> UpdateAsync(int id, Product product)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return null;

            existingProduct.ProductName = product.ProductName;
            existingProduct.UnitPrice = product.UnitPrice;
            existingProduct.UnitInStock = product.UnitInStock;
            existingProduct.QuantityPerUnit = product.QuantityPerUnit;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Description = product.Description;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.Discount = product.Discount;
            existingProduct.IsActive = product.IsActive;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingProduct;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetWithFiltersAsync(int? categoryId, decimal? minPrice, decimal? maxPrice, string? searchTerm, int page, int pageSize, string sortBy, string sortOrder)
        {
            var query = _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            if (minPrice.HasValue)
                query = query.Where(p => p.UnitPrice >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(p => p.UnitPrice <= maxPrice);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(term) || 
                    (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            // Sorting
            query = sortBy.ToLower() switch
            {
                "name" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(p => p.ProductName) : query.OrderBy(p => p.ProductName),
                "price" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(p => p.UnitPrice) : query.OrderBy(p => p.UnitPrice),
                "createdat" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Id)
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(int? categoryId, decimal? minPrice, decimal? maxPrice, string? searchTerm)
        {
            var query = _context.Products
                .Where(p => p.IsActive)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            if (minPrice.HasValue)
                query = query.Where(p => p.UnitPrice >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(p => p.UnitPrice <= maxPrice);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(term) || 
                    (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            return await query.CountAsync();
        }
    }
}
