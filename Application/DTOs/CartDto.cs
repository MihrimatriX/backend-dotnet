using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Application.DTOs
{
    public class CartDto
    {
        public int UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public int TotalItems { get; set; }
        /// <summary>Ürün tutarı (kargo öncesi).</summary>
        public decimal TotalAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        /// <summary>Ücretsiz kargo için sepete eklenecek tutar; null ise eşik aşıldı.</summary>
        public decimal? FreeShippingRemainingTry { get; set; }
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class AddToCartDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
