using EcommerceBackend.Application.DTOs;
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

        public async Task<BaseResponseDto<IEnumerable<ReviewDto>>> GetProductReviewsAsync(int productId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .Where(r => r.ProductId == productId && r.IsActive)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewDto
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
                    })
                    .ToListAsync();

                return BaseResponseDto<IEnumerable<ReviewDto>>.SuccessResult(reviews);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<IEnumerable<ReviewDto>>.ErrorResult($"Error retrieving reviews: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(int productId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ProductId == productId && r.IsActive)
                    .ToListAsync();

                if (!reviews.Any())
                {
                    return BaseResponseDto<ProductReviewSummaryDto>.SuccessResult(new ProductReviewSummaryDto
                    {
                        ProductId = productId,
                        AverageRating = 0,
                        TotalReviews = 0,
                        Rating1Count = 0,
                        Rating2Count = 0,
                        Rating3Count = 0,
                        Rating4Count = 0,
                        Rating5Count = 0
                    });
                }

                var summary = new ProductReviewSummaryDto
                {
                    ProductId = productId,
                    AverageRating = Math.Round(reviews.Average(r => r.Rating), 1),
                    TotalReviews = reviews.Count,
                    Rating1Count = reviews.Count(r => r.Rating == 1),
                    Rating2Count = reviews.Count(r => r.Rating == 2),
                    Rating3Count = reviews.Count(r => r.Rating == 3),
                    Rating4Count = reviews.Count(r => r.Rating == 4),
                    Rating5Count = reviews.Count(r => r.Rating == 5)
                };

                return BaseResponseDto<ProductReviewSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<ProductReviewSummaryDto>.ErrorResult($"Error retrieving review summary: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<ReviewDto>> GetReviewByIdAsync(int id)
        {
            try
            {
                var review = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

                if (review == null)
                {
                    return BaseResponseDto<ReviewDto>.ErrorResult("Review not found");
                }

                var reviewDto = new ReviewDto
                {
                    Id = review.Id,
                    UserId = review.UserId,
                    ProductId = review.ProductId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    Title = review.Title,
                    IsVerified = review.IsVerified,
                    IsHelpful = review.IsHelpful,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt,
                    UserName = $"{review.User.FirstName} {review.User.LastName}",
                    ProductName = review.Product.ProductName
                };

                return BaseResponseDto<ReviewDto>.SuccessResult(reviewDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<ReviewDto>.ErrorResult($"Error retrieving review: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<ReviewDto>> CreateReviewAsync(CreateReviewDto createReviewDto)
        {
            try
            {
                // Check if product exists
                var product = await _context.Products.FindAsync(createReviewDto.ProductId);
                if (product == null)
                {
                    return BaseResponseDto<ReviewDto>.ErrorResult("Product not found");
                }

                // For now, we'll use a default user ID (in a real app, this would come from authentication)
                var userId = 1; // This should be replaced with actual user authentication

                var review = new Review
                {
                    UserId = userId,
                    ProductId = createReviewDto.ProductId,
                    Rating = createReviewDto.Rating,
                    Comment = createReviewDto.Comment,
                    Title = createReviewDto.Title,
                    IsVerified = false,
                    IsHelpful = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                // Load the review with related data
                var createdReview = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .FirstAsync(r => r.Id == review.Id);

                var reviewDto = new ReviewDto
                {
                    Id = createdReview.Id,
                    UserId = createdReview.UserId,
                    ProductId = createdReview.ProductId,
                    Rating = createdReview.Rating,
                    Comment = createdReview.Comment,
                    Title = createdReview.Title,
                    IsVerified = createdReview.IsVerified,
                    IsHelpful = createdReview.IsHelpful,
                    CreatedAt = createdReview.CreatedAt,
                    UpdatedAt = createdReview.UpdatedAt,
                    UserName = $"{createdReview.User.FirstName} {createdReview.User.LastName}",
                    ProductName = createdReview.Product.ProductName
                };

                return BaseResponseDto<ReviewDto>.SuccessResult(reviewDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<ReviewDto>.ErrorResult($"Error creating review: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<ReviewDto>> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto)
        {
            try
            {
                var review = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Product)
                    .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

                if (review == null)
                {
                    return BaseResponseDto<ReviewDto>.ErrorResult("Review not found");
                }

                if (updateReviewDto.Rating.HasValue)
                    review.Rating = updateReviewDto.Rating.Value;
                
                if (updateReviewDto.Comment != null)
                    review.Comment = updateReviewDto.Comment;
                
                if (updateReviewDto.Title != null)
                    review.Title = updateReviewDto.Title;

                review.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var reviewDto = new ReviewDto
                {
                    Id = review.Id,
                    UserId = review.UserId,
                    ProductId = review.ProductId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    Title = review.Title,
                    IsVerified = review.IsVerified,
                    IsHelpful = review.IsHelpful,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt,
                    UserName = $"{review.User.FirstName} {review.User.LastName}",
                    ProductName = review.Product.ProductName
                };

                return BaseResponseDto<ReviewDto>.SuccessResult(reviewDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<ReviewDto>.ErrorResult($"Error updating review: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> DeleteReviewAsync(int id)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);

                if (review == null)
                {
                    return BaseResponseDto<string>.ErrorResult("Review not found");
                }

                review.IsActive = false;
                review.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return BaseResponseDto<string>.SuccessResult("Review deleted successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error deleting review: {ex.Message}");
            }
        }
    }
}
