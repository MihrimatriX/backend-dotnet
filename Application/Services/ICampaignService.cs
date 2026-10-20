using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface ICampaignService
    {
        Task<BaseResponseDto<List<CampaignDto>>> GetAllCampaignsAsync();
        Task<BaseResponseDto<List<CampaignDto>>> GetActiveCampaignsAsync();
        Task<BaseResponseDto<CampaignDto>> GetCampaignByIdAsync(int id);
        Task<BaseResponseDto<CampaignDto>> CreateCampaignAsync(CreateCampaignDto dto);
        Task<BaseResponseDto<CampaignDto>> UpdateCampaignAsync(int id, UpdateCampaignDto dto);
        Task<BaseResponseDto<string>> DeleteCampaignAsync(int id);
    }
}
