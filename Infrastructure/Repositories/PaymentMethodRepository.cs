using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentMethod>> GetByUserIdAsync(int userId)
        {
            return await _context.PaymentMethods
                .Where(p => p.UserId == userId && p.IsActive)
                .OrderByDescending(p => p.IsDefault)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PaymentMethod?> GetByIdAsync(int id)
        {
            return await _context.PaymentMethods
                .Where(p => p.Id == id && p.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentMethod> AddAsync(PaymentMethod paymentMethod)
        {
            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();
            return paymentMethod;
        }

        public async Task<PaymentMethod> UpdateAsync(PaymentMethod paymentMethod)
        {
            _context.PaymentMethods.Update(paymentMethod);
            await _context.SaveChangesAsync();
            return paymentMethod;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var paymentMethod = await _context.PaymentMethods
                .Where(p => p.Id == id && p.IsActive)
                .FirstOrDefaultAsync();

            if (paymentMethod == null)
                return false;

            paymentMethod.IsActive = false;
            paymentMethod.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PaymentMethods
                .AnyAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<PaymentMethod?> GetDefaultPaymentMethodAsync(int userId)
        {
            return await _context.PaymentMethods
                .Where(p => p.UserId == userId && p.IsDefault && p.IsActive)
                .FirstOrDefaultAsync();
        }
    }
}
