using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Domain.Entities
{
    public class Address : BaseEntity
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "Address title is required")]
        [StringLength(100, ErrorMessage = "Address title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Full address is required")]
        [StringLength(500, ErrorMessage = "Full address cannot exceed 500 characters")]
        public string FullAddress { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "District is required")]
        [StringLength(100, ErrorMessage = "District cannot exceed 100 characters")]
        public string District { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string PostalCode { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; } = "Turkey";
        
        public bool IsDefault { get; set; } = false;
        
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
