using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Domain.Entities
{
    public class Faq : BaseEntity
    {
        [Required(ErrorMessage = "Question is required")]
        [StringLength(500, ErrorMessage = "Question cannot exceed 500 characters")]
        public string Question { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Answer is required")]
        public string Answer { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = string.Empty;
        
        public int ViewCount { get; set; } = 0;
        public bool IsPublished { get; set; } = true;
    }
}
