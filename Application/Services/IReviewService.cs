using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IReviewService
    {
        Task<BaseResponseDto<List<ReviewDto>>> GetReviewsAdminAsync(int pageNumber = 1, int pageSize = 50);
        Task<BaseResponseDto<IEnumerable<ReviewDto>>> GetProductReviewsAsync(int productId);
        Task<BaseResponseDto<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(int productId);
        Task<BaseResponseDto<ReviewDto>> GetReviewByIdAsync(int id);
        Task<BaseResponseDto<ReviewDto>> CreateReviewAsync(CreateReviewDto createReviewDto, int userId);
        Task<BaseResponseDto<ReviewDto>> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto, int userId, bool isAdmin);
        Task<BaseResponseDto<string>> DeleteReviewAsync(int id, int userId, bool isAdmin);
    }
}
