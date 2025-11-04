using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;
using System.Security.Claims;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
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
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId != userId)
                {
                    return Forbid("You can only access your own notifications");
                }

                var result = await _notificationService.GetUserNotificationsAsync(userId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<List<NotificationDto>>.ErrorResult($"Error retrieving user notifications: {ex.Message}"));
            }
        }

        [HttpGet("{notificationId}")]
        public async Task<ActionResult<BaseResponseDto<NotificationDto>>> GetNotification(int notificationId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _notificationService.GetNotificationByIdAsync(notificationId, currentUserId);
                
                if (result.Success && result.Data != null)
                {
                    return Ok(result);
                }
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<NotificationDto>.ErrorResult($"Error retrieving notification: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseDto<NotificationDto>>> CreateNotification([FromBody] CreateNotificationDto createNotificationDto)
        {
            try
            {
                var result = await _notificationService.CreateNotificationAsync(createNotificationDto);
                
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetNotification), new { notificationId = result.Data?.Id }, result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<NotificationDto>.ErrorResult($"Error creating notification: {ex.Message}"));
            }
        }

        [HttpPut("{notificationId}")]
        public async Task<ActionResult<BaseResponseDto<NotificationDto>>> UpdateNotification(int notificationId, [FromBody] UpdateNotificationDto updateNotificationDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _notificationService.UpdateNotificationAsync(notificationId, currentUserId, updateNotificationDto);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<NotificationDto>.ErrorResult($"Error updating notification: {ex.Message}"));
            }
        }

        [HttpDelete("{notificationId}")]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteNotification(int notificationId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _notificationService.DeleteNotificationAsync(notificationId, currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error deleting notification: {ex.Message}"));
            }
        }

        [HttpPut("mark-all-read")]
        public async Task<ActionResult<BaseResponseDto<string>>> MarkAllAsRead()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _notificationService.MarkAllAsReadAsync(currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error marking notifications as read: {ex.Message}"));
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<BaseResponseDto<NotificationSummaryDto>>> GetNotificationSummary()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _notificationService.GetNotificationSummaryAsync(currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<NotificationSummaryDto>.ErrorResult($"Error retrieving notification summary: {ex.Message}"));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid user ID");
        }
    }
}
