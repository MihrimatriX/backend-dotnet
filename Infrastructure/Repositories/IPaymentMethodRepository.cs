using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public interface IPaymentMethodRepository
    {
        Task<IEnumerable<PaymentMethod>> GetByUserIdAsync(int userId);
        Task<PaymentMethod?> GetByIdAsync(int id);
        Task<PaymentMethod> AddAsync(PaymentMethod paymentMethod);
        Task<PaymentMethod> UpdateAsync(PaymentMethod paymentMethod);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<PaymentMethod?> GetDefaultPaymentMethodAsync(int userId);
    }
}
