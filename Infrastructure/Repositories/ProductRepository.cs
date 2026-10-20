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

        private IQueryable<Product> ActiveProducts =>
            _context.Products
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.SubCategory);

        public Task<Product?> GetActiveByIdAsync(int id) =>
            ActiveProducts.FirstOrDefaultAsync(p => p.Id == id);

        public Task<Product?> GetByIdAsync(int id) =>
            _context.Products
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId) =>
            await ActiveProducts
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.Id)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IReadOnlyList<Product>> SearchAsync(string searchTerm) =>
            await ApplySearch(ActiveProducts, searchTerm)
                .OrderBy(p => p.Id)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IReadOnlyList<Product>> GetFeaturedAsync(DateTime createdSince, int take) =>
            await ActiveProducts
                .Where(p => p.Discount > 20 || p.CreatedAt >= createdSince)
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();

        public async Task<IReadOnlyList<Product>> GetDiscountedAsync(int take) =>
            await ActiveProducts
                .Where(p => p.Discount > 0)
                .OrderByDescending(p => p.Discount)
                .ThenBy(p => p.Id)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();

        public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPageAsync(ProductQuery query)
        {
            var filtered = ActiveProducts;

            if (query.CategoryId.HasValue)
                filtered = filtered.Where(p => p.CategoryId == query.CategoryId);

            if (query.SubCategoryId.HasValue)
                filtered = filtered.Where(p => p.SubCategoryId == query.SubCategoryId);

            if (query.MinPrice.HasValue)
                filtered = filtered.Where(p => p.UnitPrice >= query.MinPrice);

            if (query.MaxPrice.HasValue)
                filtered = filtered.Where(p => p.UnitPrice <= query.MaxPrice);

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                filtered = ApplySearch(filtered, query.SearchTerm);

            var totalCount = await filtered.CountAsync();
            var items = await ApplySort(filtered, query.SortBy, query.Descending)
                .Skip(query.Skip)
                .Take(query.Take)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IReadOnlyDictionary<int, ProductRatingSummary>> GetRatingSummariesAsync(IReadOnlyCollection<int> productIds)
        {
            if (productIds.Count == 0)
                return new Dictionary<int, ProductRatingSummary>();

            var rows = await _context.Reviews
                .Where(r => r.IsActive && productIds.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .Select(g => new { ProductId = g.Key, Average = g.Average(r => (double)r.Rating), Count = g.Count() })
                .ToListAsync();

            return rows.ToDictionary(r => r.ProductId, r => new ProductRatingSummary(r.Average, r.Count));
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        /// <summary>Ad veya açıklamada büyük/küçük harf duyarsız arama.</summary>
        private static IQueryable<Product> ApplySearch(IQueryable<Product> query, string searchTerm)
        {
            var term = searchTerm.Trim().ToLower();
            return query.Where(p => p.ProductName.ToLower().Contains(term)
                || (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        /// <summary>
        /// Sıralama anahtarları (harf duyarsız): <c>Id</c>, <c>ProductName/name</c>, <c>UnitPrice/price</c>,
        /// <c>CreatedAt</c>, <c>Discount</c>, <c>UnitInStock/stock</c>; bilinmeyen → <c>Id</c>. Sayfalamanın kararlı olması
        /// için ikincil anahtar her zaman Id'dir.
        /// </summary>
        private static IQueryable<Product> ApplySort(IQueryable<Product> query, string? sortBy, bool descending)
        {
            var ordered = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "productname" or "name" => Order(query, p => p.ProductName, descending),
                "unitprice" or "price" => Order(query, p => p.UnitPrice, descending),
                "createdat" => Order(query, p => p.CreatedAt, descending),
                "discount" => Order(query, p => p.Discount, descending),
                "unitinstock" or "stock" => Order(query, p => p.UnitInStock, descending),
                _ => Order(query, p => p.Id, descending),
            };

            return descending ? ordered.ThenByDescending(p => p.Id) : ordered.ThenBy(p => p.Id);
        }

        private static IOrderedQueryable<Product> Order<TKey>(
            IQueryable<Product> query,
            System.Linq.Expressions.Expression<Func<Product, TKey>> key,
            bool descending) =>
            descending ? query.OrderByDescending(key) : query.OrderBy(key);
    }
}
