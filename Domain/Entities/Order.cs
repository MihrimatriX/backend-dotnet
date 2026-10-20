using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceBackend.Domain.Entities
{
    public class Order : BaseEntity
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Order number is required")]
        [StringLength(50, ErrorMessage = "Order number cannot exceed 50 characters")]
        public string OrderNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Total amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be positive")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "Pending";

        public int? ShippingAddressId { get; set; }

        public int? BillingAddressId { get; set; }

        public string? Notes { get; set; }

        public DateTime? ShippedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public int? PaymentMethodId { get; set; }

        /// <summary>İstemcinin <c>Idempotency-Key</c> başlığı (kırpılmış, küçük harf); kullanıcı başına benzersiz.</summary>
        [StringLength(128)]
        public string? IdempotencyKey { get; set; }

        [StringLength(50)]
        public string? TrackingNumber { get; set; }

        [StringLength(100)]
        public string? Carrier { get; set; }

        public DateTime? EstimatedDeliveryAt { get; set; }

        [StringLength(500)]
        public string? CancelReason { get; set; }

        [StringLength(500)]
        public string? ReturnReason { get; set; }

        public DateTime? ReturnRequestedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual PaymentMethod? PaymentMethod { get; set; }
        [NotMapped]
        public virtual Address? ShippingAddress { get; set; }
        [NotMapped]
        public virtual Address? BillingAddress { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
