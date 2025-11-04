using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Domain.Entities
{
    public class SupportMessage : BaseEntity
    {
        [Required(ErrorMessage = "Ticket ID is required")]
        public int TicketId { get; set; }
        
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; } = string.Empty;
        
        public bool IsFromSupport { get; set; } = false;
        
        // Navigation properties
        public virtual SupportTicket Ticket { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
