using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceBackend.Domain.Entities;

public class SubCategory : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string SubCategoryName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? ImageUrl { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public new bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
