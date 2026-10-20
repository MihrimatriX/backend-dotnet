using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Services;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ApiControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<BaseResponseDto<List<NotificationDto>>>> GetUserNotifications(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (CurrentUserId != userId)
                return Respond(BaseResponseDto<List<NotificationDto>>.Forbidden("You can only access your own notifications"));

            return Respond(await _notificationService.GetUserNotificationsAsync(userId, pageNumber, pageSize));
        }

        [HttpGet("summary")]
        public async Task<ActionResult<BaseResponseDto<NotificationSummaryDto>>> GetNotificationSummary() =>
            Respond(await _notificationService.GetNotificationSummaryAsync(CurrentUserId));

        [HttpGet("{notificationId}")]
        public async Task<ActionResult<BaseResponseDto<NotificationDto>>> GetNotification(int notificationId) =>
            Respond(await _notificationService.GetNotificationByIdAsync(notificationId, CurrentUserId));

        [HttpPut("mark-all-read")]
        public async Task<ActionResult<BaseResponseDto<string>>> MarkAllAsRead() =>
            Respond(await _notificationService.MarkAllAsReadAsync(CurrentUserId));

        [HttpPut("{notificationId}")]
        public async Task<ActionResult<BaseResponseDto<NotificationDto>>> UpdateNotification(int notificationId, [FromBody] UpdateNotificationDto updateNotificationDto) =>
            Respond(await _notificationService.UpdateNotificationAsync(notificationId, CurrentUserId, updateNotificationDto));

        [HttpDelete("{notificationId}")]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteNotification(int notificationId) =>
            Respond(await _notificationService.DeleteNotificationAsync(notificationId, CurrentUserId));

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<NotificationDto>>> CreateNotification([FromBody] CreateNotificationDto createNotificationDto) =>
            RespondCreated(await _notificationService.CreateNotificationAsync(createNotificationDto));

        [HttpGet("admin/user/{userId}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<List<NotificationDto>>>> AdminGetUserNotifications(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 30) =>
            Respond(await _notificationService.GetUserNotificationsAsync(userId, pageNumber, pageSize));
    }
}
