using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Repositories
{
    /// <summary>
    /// Sipariş okumaları: kalemler (ürünle), kullanıcı, ödeme yöntemi ve teslimat/fatura adresleri yüklü döner.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>Kullanıcının siparişleri, en yeni önce (salt okunur).</summary>
        Task<List<Order>> GetUserOrdersAsync(int userId);

        /// <summary>İzlenen (güncellenebilir) sipariş.</summary>
        Task<Order?> GetOrderByIdAsync(int orderId);

        Task<Order?> GetByIdempotencyKeyAsync(int userId, string idempotencyKey);

        /// <summary>Tüm siparişler, en yeni önce, sayfalı (salt okunur).</summary>
        Task<List<Order>> GetAllOrdersAsync(int skip, int take);
    }
}
