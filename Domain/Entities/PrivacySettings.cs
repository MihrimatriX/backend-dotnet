using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Domain.Entities
{
    public class PrivacySettings : BaseEntity
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        
        public bool ProfileVisibility { get; set; } = true;
        public bool ShowEmail { get; set; } = false;
        public bool ShowPhone { get; set; } = false;
        public bool AllowDataCollection { get; set; } = true;
        public bool AllowAnalytics { get; set; } = true;
        public bool AllowCookies { get; set; } = true;
        public bool AllowMarketing { get; set; } = false;
        public bool DataSharing { get; set; } = false;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
