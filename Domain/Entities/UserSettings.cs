using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Domain.Entities
{
    public class UserSettings : BaseEntity
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        
        [StringLength(10, ErrorMessage = "Language cannot exceed 10 characters")]
        public string Language { get; set; } = "tr";
        
        [StringLength(50, ErrorMessage = "Timezone cannot exceed 50 characters")]
        public string Timezone { get; set; } = "Europe/Istanbul";
        
        [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
        public string Currency { get; set; } = "TRY";
        
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool PushNotifications { get; set; } = true;
        public bool MarketingEmails { get; set; } = false;
        public bool OrderUpdates { get; set; } = true;
        public bool PriceAlerts { get; set; } = true;
        public bool StockNotifications { get; set; } = true;
        
        [StringLength(20, ErrorMessage = "Theme cannot exceed 20 characters")]
        public string Theme { get; set; } = "light";
        
        public int ItemsPerPage { get; set; } = 20;
        public bool AutoSaveCart { get; set; } = true;
        public bool ShowProductRecommendations { get; set; } = true;
        public bool EnableLocationServices { get; set; } = false;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
