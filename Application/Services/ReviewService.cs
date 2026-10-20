using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Orders;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Tüm aktif yorumlar, en yeni önce (düz dizi, §1.3 sayfalama kuralı).</summary>
        public async Task<BaseResponseDto<List<ReviewDto>>> GetReviewsAdminAsync(int pageNumber = 1, int pageSize = 50)
        {
            var paging = Paging.Normalize(pageNumber, pageSize);
            var reviews = await ActiveReviewsWithDetails()
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .ToListAsync();

            return BaseResponseDto<List<ReviewDto>>.SuccessResult(
                "Reviews retrieved successfully",
                reviews.Select(ToDto).ToList());
        }

        public async Task<BaseResponseDto<IEnumerable<ReviewDto>>> GetProductReviewsAsync(int productId)
        {
            var reviews = await ActiveReviewsWithDetails()
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .ToListAsync();

            return BaseResponseDto<IEnumerable<ReviewDto>>.SuccessResult(
                "Reviews retrieved successfully",
                reviews.Select(ToDto).ToList());
        }

        /// <summary>Yorum yoksa tüm sayaçlar sıfır.</summary>
        public async Task<BaseResponseDto<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(int productId)
        {
            var ratings = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId && r.IsActive)
                .Select(r => r.Rating)
                .ToListAsync();

            var summary = new ProductReviewSummaryDto
            {
                ProductId = productId,
                AverageRating = ratings.Count == 0 ? 0 : Math.Round(ratings.Average(), 1),
                TotalReviews = ratings.Count,
                Rating1Count = ratings.Count(r => r == 1),
                Rating2Count = ratings.Count(r => r == 2),
                Rating3Count = ratings.Count(r => r == 3),
                Rating4Count = ratings.Count(r => r == 4),
                Rating5Count = ratings.Count(r => r == 5)
            };

            return BaseResponseDto<ProductReviewSummaryDto>.SuccessResult("Review summary retrieved successfully", summary);
        }

        public async Task<BaseResponseDto<ReviewDto>> GetReviewByIdAsync(int id)
        {
            var review = await ActiveReviewsWithDetails().FirstOrDefaultAsync(r => r.Id == id);
            return review == null
                ? ReviewNotFound<ReviewDto>()
                : BaseResponseDto<ReviewDto>.SuccessResult("Review retrieved successfully", ToDto(review));
        }

        /// <summary>
        /// Kullanıcı başına ürün başına tek aktif yorum. <c>isVerified</c>: kullanıcının bu ürünü içeren
        /// iptal edilmemiş bir siparişi varsa true (§4.11).
        /// </summary>
        public async Task<BaseResponseDto<ReviewDto>> CreateReviewAsync(CreateReviewDto createReviewDto, int userId)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == createReviewDto.ProductId && p.IsActive);
            if (!productExists)
                return BaseResponseDto<ReviewDto>.Fail("Product not found", ErrorCodes.ProductNotFound);

            var duplicate = await _context.Reviews.AnyAsync(r =>
                r.UserId == userId && r.ProductId == createReviewDto.ProductId && r.IsActive);
            if (duplicate)
                return BaseResponseDto<ReviewDto>.Fail(
                    "You have already reviewed this product. Edit or remove your existing review.",
                    ErrorCodes.ReviewExists);

            var isVerified = await _context.OrderItems.AnyAsync(oi =>
                oi.ProductId == createReviewDto.ProductId
                && oi.Order.UserId == userId
                && oi.Order.Status != OrderStatuses.Cancelled);

            var review = new Review
            {
                UserId = userId,
                ProductId = createReviewDto.ProductId,
                Rating = createReviewDto.Rating,
                Comment = createReviewDto.Comment,
                Title = createReviewDto.Title,
                IsVerified = isVerified,
                IsHelpful = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var created = await ActiveReviewsWithDetails().FirstAsync(r => r.Id == review.Id);
            return BaseResponseDto<ReviewDto>.SuccessResult("Review created successfully", ToDto(created));
        }

        /// <summary>Kısmi güncelleme; yalnızca sahip veya admin.</summary>
        public async Task<BaseResponseDto<ReviewDto>> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto, int userId, bool isAdmin)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
            if (review == null)
                return ReviewNotFound<ReviewDto>();

            if (!isAdmin && review.UserId != userId)
                return BaseResponseDto<ReviewDto>.Forbidden("You can only edit your own reviews");

            if (updateReviewDto.Rating is { } rating)
                review.Rating = rating;
            if (updateReviewDto.Comment != null)
                review.Comment = updateReviewDto.Comment;
            if (updateReviewDto.Title != null)
                review.Title = updateReviewDto.Title;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BaseResponseDto<ReviewDto>.SuccessResult("Review updated successfully", ToDto(review));
        }

        /// <summary>Yumuşak silme; yalnızca sahip veya admin.</summary>
        public async Task<BaseResponseDto<string>> DeleteReviewAsync(int id, int userId, bool isAdmin)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.IsActive);
            if (review == null)
                return ReviewNotFound<string>();

            if (!isAdmin && review.UserId != userId)
                return BaseResponseDto<string>.Forbidden("You can only delete your own reviews");

            review.IsActive = false;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<string>.SuccessResult("Review deleted successfully", "Review deleted successfully");
        }

        public static ReviewDto ToDto(Review r) => new()
        {
            Id = r.Id,
            UserId = r.UserId,
            ProductId = r.ProductId,
            Rating = r.Rating,
            Comment = r.Comment,
            Title = r.Title,
            IsVerified = r.IsVerified,
            IsHelpful = r.IsHelpful,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            UserName = $"{r.User.FirstName} {r.User.LastName}",
            ProductName = r.Product.ProductName
        };

        private IQueryable<Review> ActiveReviewsWithDetails() =>
            _context.Reviews
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.IsActive);

        private static BaseResponseDto<T> ReviewNotFound<T>() =>
            BaseResponseDto<T>.NotFound("Review not found", ErrorCodes.ReviewNotFound);
    }
}
