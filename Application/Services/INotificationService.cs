using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface INotificationService
    {
        Task<BaseResponseDto<List<NotificationDto>>> GetUserNotificationsAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<BaseResponseDto<NotificationDto>> GetNotificationByIdAsync(int notificationId, int userId);
        Task<BaseResponseDto<NotificationDto>> CreateNotificationAsync(CreateNotificationDto createNotificationDto);
        Task<BaseResponseDto<NotificationDto>> UpdateNotificationAsync(int notificationId, int userId, UpdateNotificationDto updateNotificationDto);
        Task<BaseResponseDto<string>> DeleteNotificationAsync(int notificationId, int userId);
        Task<BaseResponseDto<string>> MarkAllAsReadAsync(int userId);
        Task<BaseResponseDto<NotificationSummaryDto>> GetNotificationSummaryAsync(int userId);
    }
}
