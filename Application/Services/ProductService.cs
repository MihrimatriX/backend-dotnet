using EcommerceBackend.Application.Abstractions.Caching;
using EcommerceBackend.Application.Catalog;
using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using EcommerceBackend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EcommerceBackend.Application.Services
{
    public class ProductService : IProductService
    {
        /// <summary>Vitrin listeleri (öne çıkan / indirimli) en fazla bu kadar ürün döner (§4.2).</summary>
        private const int ShowcaseLimit = 20;

        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly ICatalogReadCache _catalogCache;
        private readonly IOptionsMonitor<CatalogCacheOptions> _catalogCacheOptions;

        public ProductService(
            IProductRepository productRepository,
            ApplicationDbContext context,
            ICatalogReadCache catalogCache,
            IOptionsMonitor<CatalogCacheOptions> catalogCacheOptions)
        {
            _productRepository = productRepository;
            _context = context;
            _catalogCache = catalogCache;
            _catalogCacheOptions = catalogCacheOptions;
        }

        public async Task<BaseResponseDto<PagedResultDto<ProductDto>>> GetProductsAsync(ProductFilterDto filterDto)
        {
            var paging = Paging.Normalize(filterDto.PageNumber, filterDto.PageSize);
            var (products, totalCount) = await _productRepository.GetPageAsync(new ProductQuery(
                filterDto.CategoryId,
                filterDto.SubCategoryId,
                filterDto.MinPrice,
                filterDto.MaxPrice,
                filterDto.SearchTerm,
                filterDto.SortBy,
                Descending: string.Equals(filterDto.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase),
                paging.Skip,
                paging.PageSize));

            var pagedResult = new PagedResultDto<ProductDto>
            {
                Items = await ToDtosAsync(products),
                TotalCount = totalCount,
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
            };

            return BaseResponseDto<PagedResultDto<ProductDto>>.SuccessResult("Products retrieved successfully", pagedResult);
        }

        public async Task<BaseResponseDto<ProductDto>> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetActiveByIdAsync(id);
            if (product == null)
                return ProductNotFound<ProductDto>();

            return BaseResponseDto<ProductDto>.SuccessResult("Product retrieved successfully", await ToDtoAsync(product));
        }

        public async Task<BaseResponseDto<List<ProductDto>>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _productRepository.GetByCategoryAsync(categoryId);
            return BaseResponseDto<List<ProductDto>>.SuccessResult("Products retrieved successfully", await ToDtosAsync(products));
        }

        public async Task<BaseResponseDto<List<ProductDto>>> SearchProductsAsync(string searchTerm)
        {
            var products = await _productRepository.SearchAsync(searchTerm);
            return BaseResponseDto<List<ProductDto>>.SuccessResult("Products retrieved successfully", await ToDtosAsync(products));
        }

        /// <summary>İndirim &gt; %20 veya son 7 günde eklenmiş; en yeni önce, en fazla 20.</summary>
        public Task<BaseResponseDto<List<ProductDto>>> GetFeaturedProductsAsync() =>
            GetCachedShowcaseAsync(
                CatalogCacheKeys.FeaturedProducts,
                _catalogCacheOptions.CurrentValue.FeaturedTtlMinutes,
                () => _productRepository.GetFeaturedAsync(DateTime.UtcNow.AddDays(-7), ShowcaseLimit),
                "Featured products retrieved successfully");

        /// <summary>İndirim &gt; 0; indirim oranı yüksek olan önce, en fazla 20.</summary>
        public Task<BaseResponseDto<List<ProductDto>>> GetDiscountedProductsAsync() =>
            GetCachedShowcaseAsync(
                CatalogCacheKeys.DiscountedProducts,
                _catalogCacheOptions.CurrentValue.DiscountedTtlMinutes,
                () => _productRepository.GetDiscountedAsync(ShowcaseLimit),
                "Discounted products retrieved successfully");

        public async Task<BaseResponseDto<ProductDto>> CreateProductAsync(ProductDto productDto)
        {
            var referenceError = await ValidateReferencesAsync(productDto);
            if (referenceError != null)
                return referenceError;

            var product = new Product
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            Apply(productDto, product);
            await _productRepository.CreateAsync(product);
            await InvalidateCatalogListsAsync();

            var created = await _productRepository.GetByIdAsync(product.Id);
            return BaseResponseDto<ProductDto>.SuccessResult("Product created successfully", await ToDtoAsync(created!));
        }

        public async Task<BaseResponseDto<ProductDto>> UpdateProductAsync(int id, ProductDto productDto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return ProductNotFound<ProductDto>();

            var referenceError = await ValidateReferencesAsync(productDto);
            if (referenceError != null)
                return referenceError;

            Apply(productDto, product);
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.SaveChangesAsync();
            await InvalidateCatalogListsAsync();

            var updated = await _productRepository.GetByIdAsync(id);
            return BaseResponseDto<ProductDto>.SuccessResult("Product updated successfully", await ToDtoAsync(updated!));
        }

        public async Task<BaseResponseDto<string>> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetActiveByIdAsync(id);
            if (product == null)
                return ProductNotFound<string>();

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.SaveChangesAsync();
            await InvalidateCatalogListsAsync();

            return BaseResponseDto<string>.SuccessResult("Product deleted successfully", "Product deleted successfully");
        }

        private static BaseResponseDto<T> ProductNotFound<T>() =>
            BaseResponseDto<T>.NotFound("Product not found", ErrorCodes.ProductNotFound);

        /// <summary>Kategori aktif olmalı; alt kategori verildiyse aynı kategoriye ait ve aktif olmalı.</summary>
        private async Task<BaseResponseDto<ProductDto>?> ValidateReferencesAsync(ProductDto dto)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.IsActive))
                return BaseResponseDto<ProductDto>.Fail("Category not found", ErrorCodes.CategoryNotFound);

            if (dto.SubCategoryId is { } subCategoryId
                && !await _context.SubCategories.AnyAsync(s => s.Id == subCategoryId && s.IsActive && s.CategoryId == dto.CategoryId))
            {
                return BaseResponseDto<ProductDto>.Fail("SubCategory not found", ErrorCodes.SubCategoryNotFound);
            }

            return null;
        }

        private static void Apply(ProductDto dto, Product product)
        {
            product.ProductName = dto.ProductName.Trim();
            product.UnitPrice = dto.UnitPrice;
            product.UnitInStock = dto.UnitInStock;
            product.QuantityPerUnit = dto.QuantityPerUnit;
            product.CategoryId = dto.CategoryId;
            product.SubCategoryId = dto.SubCategoryId;
            product.Description = dto.Description;
            product.ImageUrl = dto.ImageUrl;
            product.Discount = dto.Discount;
            product.IsActive = dto.IsActive;
        }

        private async Task<BaseResponseDto<List<ProductDto>>> GetCachedShowcaseAsync(
            string cacheKey,
            int ttlMinutes,
            Func<Task<IReadOnlyList<Product>>> load,
            string message)
        {
            var cached = await _catalogCache.GetAsync<List<ProductDto>>(cacheKey);
            if (cached is not null)
                return BaseResponseDto<List<ProductDto>>.SuccessResult(message, cached);

            var productDtos = await ToDtosAsync(await load());
            await _catalogCache.SetAsync(cacheKey, productDtos, TimeSpan.FromMinutes(Math.Max(1, ttlMinutes)));
            return BaseResponseDto<List<ProductDto>>.SuccessResult(message, productDtos);
        }

        private async Task<ProductDto> ToDtoAsync(Product product) => (await ToDtosAsync([product]))[0];

        private async Task<List<ProductDto>> ToDtosAsync(IReadOnlyList<Product> products)
        {
            var ratings = await _productRepository.GetRatingSummariesAsync(products.Select(p => p.Id).ToList());
            return products
                .Select(p => ToDto(p, ratings.TryGetValue(p.Id, out var rating) ? rating : null))
                .ToList();
        }

        private static ProductDto ToDto(Product product, ProductRatingSummary? rating) => new()
        {
            Id = product.Id,
            ProductName = product.ProductName,
            UnitPrice = product.UnitPrice,
            UnitInStock = product.UnitInStock,
            QuantityPerUnit = product.QuantityPerUnit,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.CategoryName,
            SubCategoryId = product.SubCategoryId,
            SubCategoryName = product.SubCategory?.SubCategoryName,
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            Discount = product.Discount,
            IsActive = product.IsActive,
            AverageRating = rating is null ? 0 : Math.Round(rating.AverageRating, 1, MidpointRounding.AwayFromZero),
            TotalReviews = rating?.TotalReviews ?? 0,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
        };

        private async Task InvalidateCatalogListsAsync()
        {
            await _catalogCache.RemoveAsync(CatalogCacheKeys.FeaturedProducts);
            await _catalogCache.RemoveAsync(CatalogCacheKeys.DiscountedProducts);
        }
    }
}
