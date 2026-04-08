using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(long id);
        Task<User> CreateAsync(User user);
        Task<bool> ExistsByEmailAsync(string email);
    }
}
