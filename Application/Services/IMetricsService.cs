using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IMetricsService
    {
        Task<BaseResponseDto<object>> GetDashboardMetricsAsync();
        Task<BaseResponseDto<object>> GetProductMetricsAsync();
        Task<BaseResponseDto<object>> GetOrderMetricsAsync();
        Task<BaseResponseDto<object>> GetUserMetricsAsync();
        Task<BaseResponseDto<object>> GetSalesMetricsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<BaseResponseDto<object>> GetCategoryMetricsAsync();
        Task<BaseResponseDto<object>> GetReviewMetricsAsync();
        Task<BaseResponseDto<object>> GetCampaignMetricsAsync();
    }
}
