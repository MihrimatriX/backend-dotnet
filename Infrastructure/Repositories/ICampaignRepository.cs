using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Infrastructure.Repositories
{
    public interface ICampaignRepository
    {
        Task<IEnumerable<Campaign>> GetAllAsync();
        Task<Campaign?> GetByIdAsync(int id);
        Task<IEnumerable<Campaign>> GetActiveAsync();
        Task<Campaign> AddAsync(Campaign campaign);
        Task<Campaign> UpdateAsync(Campaign campaign);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
