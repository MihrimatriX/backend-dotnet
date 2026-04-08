using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<Order> AddAsync(Order order);
        Task<Order> UpdateAsync(Order order);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Order>> GetByStatusAsync(string status);
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Order>> GetUserOrdersAsync(int userId);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<Order> CreateAsync(Order order);
        Task<List<Order>> GetAllOrdersAsync();
    }
}
