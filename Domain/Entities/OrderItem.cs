using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        [Required(ErrorMessage = "Order ID is required")]
        public int OrderId { get; set; }
        
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive")]
        public int Quantity { get; set; }
        
        [Required(ErrorMessage = "Unit price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be positive")]
        public decimal UnitPrice { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Discount must be positive")]
        public decimal Discount { get; set; } = 0;
        
        public decimal TotalPrice => (UnitPrice - Discount) * Quantity;
        
        // Navigation properties
        public virtual Order Order { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
