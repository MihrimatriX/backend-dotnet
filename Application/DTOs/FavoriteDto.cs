using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Application.DTOs
{
    public class FavoriteDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal? ProductDiscount { get; set; }
        public string? ProductCategory { get; set; }
        public bool ProductInStock { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddToFavoritesDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }
    }

    public class RemoveFromFavoritesDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }
    }
}