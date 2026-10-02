using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Application.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        /// <summary>Ürün satırlarının toplamı (kargo hariç).</summary>
        public decimal SubtotalAmount { get; set; }
        public decimal ShippingFee { get; set; }
        /// <summary>Ara toplam + kargo (ödenecek toplam).</summary>
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public AddressDto? ShippingAddress { get; set; }
        public AddressDto? BillingAddress { get; set; }
        /// <summary>Maskeli ödeme yöntemi.</summary>
        public PaymentMethodDto? PaymentMethod { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? EstimatedDeliveryAt { get; set; }
        public string? CancelReason { get; set; }
        public string? ReturnReason { get; set; }
        public DateTime? ReturnRequestedAt { get; set; }
        /// <summary>Demo lojistik açıkken ilerletilebilir siparişlerde <c>DEMO_ADVANCE_FULFILLMENT</c> (§5.5).</summary>
        public string? DemoNextAction { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        /// <summary>Sipariş anındaki indirimli birim fiyat.</summary>
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    /// <summary>Sipariş oluşturma (§5.2). Boş <c>items</c> → 400 <c>EMPTY_ORDER</c> (servis kuralı).</summary>
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Shipping address is required")]
        public int ShippingAddressId { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public int PaymentMethodId { get; set; }

        [Required(ErrorMessage = "Order items are required")]
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();

        public string? Notes { get; set; }
    }

    public class CreateOrderItemDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }

    /// <summary>Müşteri iptali; gövde isteğe bağlıdır.</summary>
    public class CancelOrderDto
    {
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }
    }

    public class ReturnRequestDto
    {
        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;
    }
}
