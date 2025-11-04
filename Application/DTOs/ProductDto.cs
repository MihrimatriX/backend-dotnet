using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
        public string ProductName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }
        
        [Required(ErrorMessage = "Unit in stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Unit in stock cannot be negative")]
        public int UnitInStock { get; set; }
        
        [Required(ErrorMessage = "Quantity per unit is required")]
        [StringLength(50, ErrorMessage = "Quantity per unit cannot exceed 50 characters")]
        public string QuantityPerUnit { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Category ID is required")]
        public int CategoryId { get; set; }
        
        public string? CategoryName { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
        
        public string? ImageUrl { get; set; }
        
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public int Discount { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        // Review information
        public double? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
