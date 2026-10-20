using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Domain.Entities
{
    /// <summary>Kullanıcı başına (1:1) güvenlik ayarları (§4.13); kayıt yoksa varsayılanlar döner.</summary>
    public class SecuritySettings : BaseEntity
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool LoginAlerts { get; set; } = true;
        public bool TwoFactorRequired { get; set; } = false;

        /// <summary>Dakika.</summary>
        public int SessionTimeout { get; set; } = 30;

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
