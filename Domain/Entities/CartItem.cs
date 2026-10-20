using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Domain.Entities;

/// <summary>
/// Kullanıcı sepet satırı (kalıcı; giriş yapan müşteri).
/// </summary>
public class CartItem : BaseEntity
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}