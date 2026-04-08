using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface ICampaignService
    {
        Task<BaseResponseDto<List<CampaignDto>>> GetAllCampaignsAsync();
        Task<BaseResponseDto<CampaignDto>> GetCampaignByIdAsync(int id);
        Task<BaseResponseDto<List<CampaignDto>>> GetActiveCampaignsAsync();
        Task<BaseResponseDto<CampaignDto>> CreateCampaignAsync(CampaignDto campaignDto);
        Task<BaseResponseDto<CampaignDto>> UpdateCampaignAsync(int id, CampaignDto campaignDto);
        Task<BaseResponseDto<bool>> DeleteCampaignAsync(int id);
        Task<BaseResponseDto<bool>> ActivateCampaignAsync(int id);
        Task<BaseResponseDto<bool>> DeactivateCampaignAsync(int id);
    }
}
