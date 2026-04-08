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
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
}

public class UpdateSubCategoryDto
{
    public int Id { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; }
}
