using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponseDto<List<NotificationDto>>> GetUserNotificationsAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.IsActive)
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        UserId = n.UserId,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        ActionUrl = n.ActionUrl,
                        IsRead = n.IsRead,
                        ReadAt = n.ReadAt,
                        IsActive = n.IsActive,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt
                    })
                    .ToListAsync();

                return BaseResponseDto<List<NotificationDto>>.SuccessResult("Notifications retrieved successfully", notifications);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<List<NotificationDto>>.ErrorResult($"Error retrieving notifications: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<NotificationDto>> GetNotificationByIdAsync(int notificationId, int userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .Where(n => n.Id == notificationId && n.UserId == userId && n.IsActive)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        UserId = n.UserId,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        ActionUrl = n.ActionUrl,
                        IsRead = n.IsRead,
                        ReadAt = n.ReadAt,
                        IsActive = n.IsActive,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (notification == null)
                {
                    return BaseResponseDto<NotificationDto>.ErrorResult("Notification not found");
                }

                return BaseResponseDto<NotificationDto>.SuccessResult("Notification retrieved successfully", notification);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<NotificationDto>.ErrorResult($"Error retrieving notification: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<NotificationDto>> CreateNotificationAsync(CreateNotificationDto createNotificationDto)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = createNotificationDto.UserId,
                    Title = createNotificationDto.Title,
                    Message = createNotificationDto.Message,
                    Type = createNotificationDto.Type,
                    ActionUrl = createNotificationDto.ActionUrl,
                    IsRead = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var notificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    ActionUrl = notification.ActionUrl,
                    IsRead = notification.IsRead,
                    ReadAt = notification.ReadAt,
                    IsActive = notification.IsActive,
                    CreatedAt = notification.CreatedAt,
                    UpdatedAt = notification.UpdatedAt
                };

                return BaseResponseDto<NotificationDto>.SuccessResult("Notification created successfully", notificationDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<NotificationDto>.ErrorResult($"Error creating notification: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<NotificationDto>> UpdateNotificationAsync(int notificationId, int userId, UpdateNotificationDto updateNotificationDto)
        {
            try
            {
                var notification = await _context.Notifications
                    .Where(n => n.Id == notificationId && n.UserId == userId && n.IsActive)
                    .FirstOrDefaultAsync();

                if (notification == null)
                {
                    return BaseResponseDto<NotificationDto>.ErrorResult("Notification not found");
                }

                notification.IsRead = updateNotificationDto.IsRead;
                if (updateNotificationDto.IsRead && !notification.IsRead)
                {
                    notification.ReadAt = DateTime.UtcNow;
                }
                notification.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var notificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    ActionUrl = notification.ActionUrl,
                    IsRead = notification.IsRead,
                    ReadAt = notification.ReadAt,
                    IsActive = notification.IsActive,
                    CreatedAt = notification.CreatedAt,
                    UpdatedAt = notification.UpdatedAt
                };

                return BaseResponseDto<NotificationDto>.SuccessResult("Notification updated successfully", notificationDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<NotificationDto>.ErrorResult($"Error updating notification: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> DeleteNotificationAsync(int notificationId, int userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .Where(n => n.Id == notificationId && n.UserId == userId && n.IsActive)
                    .FirstOrDefaultAsync();

                if (notification == null)
                {
                    return BaseResponseDto<string>.ErrorResult("Notification not found");
                }

                notification.IsActive = false;
                notification.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return BaseResponseDto<string>.SuccessResult("Notification deleted successfully", "Notification deleted successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error deleting notification: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> MarkAllAsReadAsync(int userId)
        {
            try
            {
                var unreadNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.IsActive && !n.IsRead)
                    .ToListAsync();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    notification.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return BaseResponseDto<string>.SuccessResult("All notifications marked as read", "All notifications marked as read");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error marking notifications as read: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<NotificationSummaryDto>> GetNotificationSummaryAsync(int userId)
        {
            try
            {
                var totalNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.IsActive)
                    .CountAsync();

                var unreadNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.IsActive && !n.IsRead)
                    .CountAsync();

                var recentNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.IsActive)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        UserId = n.UserId,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        ActionUrl = n.ActionUrl,
                        IsRead = n.IsRead,
                        ReadAt = n.ReadAt,
                        IsActive = n.IsActive,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt
                    })
                    .ToListAsync();

                var summary = new NotificationSummaryDto
                {
                    TotalNotifications = totalNotifications,
                    UnreadNotifications = unreadNotifications,
                    RecentNotifications = recentNotifications
                };

                return BaseResponseDto<NotificationSummaryDto>.SuccessResult("Notification summary retrieved successfully", summary);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<NotificationSummaryDto>.ErrorResult($"Error retrieving notification summary: {ex.Message}");
            }
        }
    }
}
