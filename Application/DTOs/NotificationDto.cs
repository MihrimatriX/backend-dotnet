using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Application.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateNotificationDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Action URL cannot exceed 100 characters")]
        public string? ActionUrl { get; set; }
    }

    public class UpdateNotificationDto
    {
        public bool IsRead { get; set; }
    }

    public class NotificationSummaryDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public List<NotificationDto> RecentNotifications { get; set; } = new List<NotificationDto>();
    }
}
