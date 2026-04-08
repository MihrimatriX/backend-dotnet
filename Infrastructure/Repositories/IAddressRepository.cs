using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetByUserIdAsync(int userId);
        Task<Address?> GetByIdAsync(int id);
        Task<Address> AddAsync(Address address);
        Task<Address> UpdateAsync(Address address);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<Address?> GetDefaultAddressAsync(int userId);
    }
}
