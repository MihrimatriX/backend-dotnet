using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Domain.Entities
{
    public class LoginHistory : BaseEntity
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "Login time is required")]
        public DateTime LoginAt { get; set; }
        
        [StringLength(45, ErrorMessage = "IP address cannot exceed 45 characters")]
        public string? IpAddress { get; set; }
        
        [StringLength(500, ErrorMessage = "User agent cannot exceed 500 characters")]
        public string? UserAgent { get; set; }
        
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
        public string? Location { get; set; }
        
        public bool IsSuccessful { get; set; } = true;
        
        [StringLength(200, ErrorMessage = "Failure reason cannot exceed 200 characters")]
        public string? FailureReason { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
