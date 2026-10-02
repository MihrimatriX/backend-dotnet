using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Application.DTOs;

public class SubCategoryDto
{
    public int Id { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSubCategoryDto
{
    [Required(ErrorMessage = "SubCategory name is required")]
    [StringLength(100, ErrorMessage = "SubCategory name cannot exceed 100 characters")]
    public string SubCategoryName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [StringLength(200, ErrorMessage = "Image URL cannot exceed 200 characters")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Category ID is required")]
    public int CategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateSubCategoryDto
{
    /// <summary>Gönderilmişse rotadaki id ile aynı olmalı (400 <c>ID_MISMATCH</c>).</summary>
    public int? Id { get; set; }

    [Required(ErrorMessage = "SubCategory name is required")]
    [StringLength(100, ErrorMessage = "SubCategory name cannot exceed 100 characters")]
    public string SubCategoryName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [StringLength(200, ErrorMessage = "Image URL cannot exceed 200 characters")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Category ID is required")]
    public int CategoryId { get; set; }

    /// <summary>Gönderilmezse (null) değişmez.</summary>
    public bool? IsActive { get; set; }
}
