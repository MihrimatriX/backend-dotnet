using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Order> OrdersWithDetails =>
            _context.Orders
                .Where(o => o.IsActive)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.PaymentMethod)
                .AsSplitQuery();

        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            var orders = await OrdersWithDetails
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ThenByDescending(o => o.Id)
                .ToListAsync();
            await AttachOrderAddressesAsync(orders);
            return orders;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            var order = await OrdersWithDetails.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null)
                await AttachOrderAddressesAsync([order]);
            return order;
        }

        public async Task<Order?> GetByIdempotencyKeyAsync(int userId, string idempotencyKey)
        {
            var order = await OrdersWithDetails
                .FirstOrDefaultAsync(o => o.UserId == userId && o.IdempotencyKey == idempotencyKey);
            if (order != null)
                await AttachOrderAddressesAsync([order]);
            return order;
        }

        public async Task<List<Order>> GetAllOrdersAsync(int skip, int take)
        {
            var orders = await OrdersWithDetails
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .ThenByDescending(o => o.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
            await AttachOrderAddressesAsync(orders);
            return orders;
        }

        /// <summary>
        /// ShippingAddress / BillingAddress entity üzerinde [NotMapped]; EF Include kullanılamaz.
        /// Adresler (silinmiş olsalar da) sipariş anındaki kayıt olarak tek sorguda yüklenir.
        /// </summary>
        private async Task AttachOrderAddressesAsync(IReadOnlyCollection<Order> orders)
        {
            var ids = orders
                .SelectMany(o => new[] { o.ShippingAddressId, o.BillingAddressId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();
            if (ids.Count == 0)
                return;

            var map = await _context.Addresses
                .AsNoTracking()
                .Where(a => ids.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id);

            foreach (var o in orders)
            {
                if (o.ShippingAddressId is { } sid && map.TryGetValue(sid, out var ship))
                    o.ShippingAddress = ship;
                if (o.BillingAddressId is { } bid && map.TryGetValue(bid, out var bill))
                    o.BillingAddress = bill;
            }
        }
    }
}
