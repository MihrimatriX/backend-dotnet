using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Application.DTOs
{
    public class UserSettingsDto
    {
        public int UserId { get; set; }
        public string Language { get; set; } = "tr";
        public string Timezone { get; set; } = "Europe/Istanbul";
        public string Currency { get; set; } = "TRY";
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool PushNotifications { get; set; } = true;
        public bool MarketingEmails { get; set; } = false;
        public bool OrderUpdates { get; set; } = true;
        public bool PriceAlerts { get; set; } = true;
        public bool StockNotifications { get; set; } = true;
        public string Theme { get; set; } = "light";
        public int ItemsPerPage { get; set; } = 20;
        public bool AutoSaveCart { get; set; } = true;
        public bool ShowProductRecommendations { get; set; } = true;
        public bool EnableLocationServices { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateUserSettingsDto
    {
        [StringLength(10, ErrorMessage = "Language cannot exceed 10 characters")]
        public string? Language { get; set; }

        [StringLength(50, ErrorMessage = "Timezone cannot exceed 50 characters")]
        public string? Timezone { get; set; }

        [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
        public string? Currency { get; set; }

        public bool? EmailNotifications { get; set; }
        public bool? SmsNotifications { get; set; }
        public bool? PushNotifications { get; set; }
        public bool? MarketingEmails { get; set; }
        public bool? OrderUpdates { get; set; }
        public bool? PriceAlerts { get; set; }
        public bool? StockNotifications { get; set; }

        [StringLength(20, ErrorMessage = "Theme cannot exceed 20 characters")]
        public string? Theme { get; set; }

        [Range(5, 100, ErrorMessage = "Items per page must be between 5 and 100")]
        public int? ItemsPerPage { get; set; }

        public bool? AutoSaveCart { get; set; }
        public bool? ShowProductRecommendations { get; set; }
        public bool? EnableLocationServices { get; set; }
    }

    public class PrivacySettingsDto
    {
        public int UserId { get; set; }
        public bool ProfileVisibility { get; set; } = true;
        public bool ShowEmail { get; set; } = false;
        public bool ShowPhone { get; set; } = false;
        public bool AllowDataCollection { get; set; } = true;
        public bool AllowAnalytics { get; set; } = true;
        public bool AllowCookies { get; set; } = true;
        public bool AllowMarketing { get; set; } = false;
        public bool DataSharing { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdatePrivacySettingsDto
    {
        public bool? ProfileVisibility { get; set; }
        public bool? ShowEmail { get; set; }
        public bool? ShowPhone { get; set; }
        public bool? AllowDataCollection { get; set; }
        public bool? AllowAnalytics { get; set; }
        public bool? AllowCookies { get; set; }
        public bool? AllowMarketing { get; set; }
        public bool? DataSharing { get; set; }
    }
}
