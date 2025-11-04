using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IReviewService
    {
        Task<BaseResponseDto<IEnumerable<ReviewDto>>> GetProductReviewsAsync(int productId);
        Task<BaseResponseDto<ProductReviewSummaryDto>> GetProductReviewSummaryAsync(int productId);
        Task<BaseResponseDto<ReviewDto>> GetReviewByIdAsync(int id);
        Task<BaseResponseDto<ReviewDto>> CreateReviewAsync(CreateReviewDto createReviewDto);
        Task<BaseResponseDto<ReviewDto>> UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto);
        Task<BaseResponseDto<string>> DeleteReviewAsync(int id);
    }
}
