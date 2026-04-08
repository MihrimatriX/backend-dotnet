using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ApplicationDbContext _context;

        public AddressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Address>> GetByUserIdAsync(int userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId && a.IsActive)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Address?> GetByIdAsync(int id)
        {
            return await _context.Addresses
                .Where(a => a.Id == id && a.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<Address> AddAsync(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<Address> UpdateAsync(Address address)
        {
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var address = await _context.Addresses
                .Where(a => a.Id == id && a.IsActive)
                .FirstOrDefaultAsync();

            if (address == null)
                return false;

            address.IsActive = false;
            address.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Addresses
                .AnyAsync(a => a.Id == id && a.IsActive);
        }

        public async Task<Address?> GetDefaultAddressAsync(int userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId && a.IsDefault && a.IsActive)
                .FirstOrDefaultAsync();
        }
    }
}
