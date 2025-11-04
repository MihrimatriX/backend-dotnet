using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceBackend.Domain.Entities
{
    public class Product : BaseEntity
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
        public string ProductName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        [Column(TypeName = "decimal(12,2)")]
        public decimal UnitPrice { get; set; }
        
        [Required(ErrorMessage = "Unit in stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Unit in stock cannot be negative")]
        public int UnitInStock { get; set; }
        
        [Required(ErrorMessage = "Quantity per unit is required")]
        [StringLength(50, ErrorMessage = "Quantity per unit cannot exceed 50 characters")]
        public string QuantityPerUnit { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;
        
        public int? SubCategoryId { get; set; }
        
        [ForeignKey("SubCategoryId")]
        public virtual SubCategory? SubCategory { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
        
        public string? ImageUrl { get; set; }
        
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public int Discount { get; set; } = 0;
    }
}
