using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Application.DTOs
{
    public class CampaignDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? Description { get; set; }
        public int Discount { get; set; }
        public string? ImageUrl { get; set; }
        public string? BackgroundColor { get; set; }
        public string? TimeLeft { get; set; }
        public string? ButtonText { get; set; }
        public string? ButtonHref { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateCampaignDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Subtitle cannot exceed 300 characters")]
        public string? Subtitle { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public int Discount { get; set; } = 0;

        public string? ImageUrl { get; set; }

        public string? BackgroundColor { get; set; }

        public string? TimeLeft { get; set; }

        [StringLength(50, ErrorMessage = "Button text cannot exceed 50 characters")]
        public string? ButtonText { get; set; }

        public string? ButtonHref { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCampaignDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Subtitle cannot exceed 300 characters")]
        public string? Subtitle { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public int Discount { get; set; }

        public string? ImageUrl { get; set; }

        public string? BackgroundColor { get; set; }

        public string? TimeLeft { get; set; }

        [StringLength(50, ErrorMessage = "Button text cannot exceed 50 characters")]
        public string? ButtonText { get; set; }

        public string? ButtonHref { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}
