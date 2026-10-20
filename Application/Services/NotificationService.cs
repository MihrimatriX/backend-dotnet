using EcommerceBackend.Application.Common;
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

        public async Task<BaseResponseDto<List<NotificationDto>>> GetUserNotificationsAsync(int userId, int pageNumber, int pageSize)
        {
            var paging = Paging.Normalize(pageNumber, pageSize);
            var notifications = await ActiveFor(userId)
                .OrderByDescending(n => n.CreatedAt)
                .ThenByDescending(n => n.Id)
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .ToListAsync();

            return BaseResponseDto<List<NotificationDto>>.SuccessResult(
                "Notifications retrieved successfully",
                notifications.Select(ToDto).ToList());
        }

        public async Task<BaseResponseDto<NotificationDto>> GetNotificationByIdAsync(int notificationId, int userId)
        {
            var notification = await ActiveFor(userId).FirstOrDefaultAsync(n => n.Id == notificationId);
            return notification == null
                ? NotificationNotFound<NotificationDto>()
                : BaseResponseDto<NotificationDto>.SuccessResult("Notification retrieved successfully", ToDto(notification));
        }

        public async Task<BaseResponseDto<NotificationDto>> CreateNotificationAsync(CreateNotificationDto createNotificationDto)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == createNotificationDto.UserId && u.IsActive))
                return BaseResponseDto<NotificationDto>.Fail("User not found", ErrorCodes.UserNotFound);

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

            return BaseResponseDto<NotificationDto>.SuccessResult("Notification created successfully", ToDto(notification));
        }

        /// <summary>Okundu yapılırsa <c>readAt</c> = şimdi, okunmadı yapılırsa <c>readAt</c> = null.</summary>
        public async Task<BaseResponseDto<NotificationDto>> UpdateNotificationAsync(int notificationId, int userId, UpdateNotificationDto updateNotificationDto)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && n.IsActive);
            if (notification == null)
                return NotificationNotFound<NotificationDto>();

            if (updateNotificationDto.IsRead && !notification.IsRead)
                notification.ReadAt = DateTime.UtcNow;
            else if (!updateNotificationDto.IsRead)
                notification.ReadAt = null;
            notification.IsRead = updateNotificationDto.IsRead;
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BaseResponseDto<NotificationDto>.SuccessResult("Notification updated successfully", ToDto(notification));
        }

        public async Task<BaseResponseDto<string>> DeleteNotificationAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && n.IsActive);
            if (notification == null)
                return NotificationNotFound<string>();

            notification.IsActive = false;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<string>.SuccessResult("Notification deleted successfully", "Notification deleted successfully");
        }

        public async Task<BaseResponseDto<string>> MarkAllAsReadAsync(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsActive && !n.IsRead)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var notification in unread)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
                notification.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

            return BaseResponseDto<string>.SuccessResult("All notifications marked as read", "All notifications marked as read");
        }

        /// <summary>Toplam, okunmamış ve en yeni 5 bildirim.</summary>
        public async Task<BaseResponseDto<NotificationSummaryDto>> GetNotificationSummaryAsync(int userId)
        {
            var recent = await ActiveFor(userId)
                .OrderByDescending(n => n.CreatedAt)
                .ThenByDescending(n => n.Id)
                .Take(5)
                .ToListAsync();

            var summary = new NotificationSummaryDto
            {
                TotalNotifications = await ActiveFor(userId).CountAsync(),
                UnreadNotifications = await ActiveFor(userId).CountAsync(n => !n.IsRead),
                RecentNotifications = recent.Select(ToDto).ToList()
            };

            return BaseResponseDto<NotificationSummaryDto>.SuccessResult("Notification summary retrieved successfully", summary);
        }

        private IQueryable<Notification> ActiveFor(int userId) =>
            _context.Notifications.AsNoTracking().Where(n => n.UserId == userId && n.IsActive);

        private static BaseResponseDto<T> NotificationNotFound<T>() =>
            BaseResponseDto<T>.NotFound("Notification not found", ErrorCodes.NotificationNotFound);

        private static NotificationDto ToDto(Notification n) => new()
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
        };
    }
}
