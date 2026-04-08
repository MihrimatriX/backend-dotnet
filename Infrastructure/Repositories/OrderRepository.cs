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

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Where(o => o.IsActive)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Where(o => o.Id == id && o.IsActive)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && o.IsActive)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Where(o => o.OrderNumber == orderNumber && o.IsActive)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync();
        }

        public async Task<Order> AddAsync(Order order)
        {
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

            if (order == null)
                return false;

            order.IsActive = false;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Orders
                .AnyAsync(o => o.Id == id && o.IsActive);
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
        {
            return await _context.Orders
                .Where(o => o.Status == status && o.IsActive)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.IsActive)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId && o.IsActive)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.PaymentMethod)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            await AttachOrderAddressesAsync(orders);
            return orders;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Where(o => o.Id == orderId && o.IsActive)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.PaymentMethod)
                .FirstOrDefaultAsync();
            if (order != null)
                await AttachOrderAddressesAsync(new[] { order });
            return order;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Where(o => o.IsActive)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.PaymentMethod)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            await AttachOrderAddressesAsync(orders);
            return orders;
        }

        /// <summary>
        /// ShippingAddress / BillingAddress entity üzerinde [NotMapped]; EF Include kullanılamaz.
        /// </summary>
        private async Task AttachOrderAddressesAsync(IEnumerable<Order> orders)
        {
            var list = orders as IList<Order> ?? orders.ToList();
            if (list.Count == 0)
                return;

            var idSet = new HashSet<int>();
            foreach (var o in list)
            {
                if (o.ShippingAddressId is { } s)
                    idSet.Add(s);
                if (o.BillingAddressId is { } b)
                    idSet.Add(b);
            }

            if (idSet.Count == 0)
                return;

            var map = await _context.Addresses
                .AsNoTracking()
                .Where(a => idSet.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id);

            foreach (var o in list)
            {
                if (o.ShippingAddressId is { } sid && map.TryGetValue(sid, out var ship))
                    o.ShippingAddress = ship;
                if (o.BillingAddressId is { } bid && map.TryGetValue(bid, out var bill))
                    o.BillingAddress = bill;
            }
        }
    }
}
