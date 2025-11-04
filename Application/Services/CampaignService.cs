using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CampaignService> _logger;

        public CampaignService(ApplicationDbContext context, ILogger<CampaignService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BaseResponseDto<List<CampaignDto>>> GetAllCampaignsAsync()
        {
            try
            {
                var campaigns = await _context.Campaigns
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CampaignDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Subtitle = c.Subtitle,
                        Description = c.Description,
                        ButtonText = c.ButtonText,
                        Discount = c.Discount,
                        IsActive = c.IsActive,
                        ImageUrl = c.ImageUrl,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                return new BaseResponseDto<List<CampaignDto>>
                {
                    Success = true,
                    Data = campaigns,
                    Message = "Campaigns retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaigns");
                return new BaseResponseDto<List<CampaignDto>>
                {
                    Success = false,
                    Message = "Error retrieving campaigns"
                };
            }
        }

        public async Task<BaseResponseDto<CampaignDto>> GetCampaignByIdAsync(int id)
        {
            try
            {
                var campaign = await _context.Campaigns
                    .Where(c => c.Id == id && c.IsActive)
                    .Select(c => new CampaignDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Subtitle = c.Subtitle,
                        Description = c.Description,
                        ButtonText = c.ButtonText,
                        Discount = c.Discount,
                        IsActive = c.IsActive,
                        ImageUrl = c.ImageUrl,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (campaign == null)
                {
                    return new BaseResponseDto<CampaignDto>
                    {
                        Success = false,
                        Message = "Campaign not found"
                    };
                }

                return new BaseResponseDto<CampaignDto>
                {
                    Success = true,
                    Data = campaign,
                    Message = "Campaign retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaign {CampaignId}", id);
                return new BaseResponseDto<CampaignDto>
                {
                    Success = false,
                    Message = "Error retrieving campaign"
                };
            }
        }

        public async Task<BaseResponseDto<List<CampaignDto>>> GetActiveCampaignsAsync()
        {
            try
            {
                var campaigns = await _context.Campaigns
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CampaignDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Subtitle = c.Subtitle,
                        Description = c.Description,
                        ButtonText = c.ButtonText,
                        Discount = c.Discount,
                        IsActive = c.IsActive,
                        ImageUrl = c.ImageUrl,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                return new BaseResponseDto<List<CampaignDto>>
                {
                    Success = true,
                    Data = campaigns,
                    Message = "Active campaigns retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active campaigns");
                return new BaseResponseDto<List<CampaignDto>>
                {
                    Success = false,
                    Message = "Error retrieving active campaigns"
                };
            }
        }

        public async Task<BaseResponseDto<CampaignDto>> CreateCampaignAsync(CampaignDto campaignDto)
        {
            try
            {
                var campaign = new Campaign
                {
                    Title = campaignDto.Title,
                    Subtitle = campaignDto.Subtitle,
                    Description = campaignDto.Description,
                    ButtonText = campaignDto.ButtonText,
                    Discount = campaignDto.Discount,
                    IsActive = campaignDto.IsActive,
                    ImageUrl = campaignDto.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Campaigns.Add(campaign);
                await _context.SaveChangesAsync();

                campaignDto.Id = campaign.Id;
                campaignDto.CreatedAt = campaign.CreatedAt;
                campaignDto.UpdatedAt = campaign.UpdatedAt;

                return new BaseResponseDto<CampaignDto>
                {
                    Success = true,
                    Data = campaignDto,
                    Message = "Campaign created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating campaign");
                return new BaseResponseDto<CampaignDto>
                {
                    Success = false,
                    Message = "Error creating campaign"
                };
            }
        }

        public async Task<BaseResponseDto<CampaignDto>> UpdateCampaignAsync(int id, CampaignDto campaignDto)
        {
            try
            {
                var campaign = await _context.Campaigns
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

                if (campaign == null)
                {
                    return new BaseResponseDto<CampaignDto>
                    {
                        Success = false,
                        Message = "Campaign not found"
                    };
                }

                campaign.Title = campaignDto.Title;
                campaign.Subtitle = campaignDto.Subtitle;
                campaign.Description = campaignDto.Description;
                campaign.ButtonText = campaignDto.ButtonText;
                campaign.Discount = campaignDto.Discount;
                campaign.IsActive = campaignDto.IsActive;
                campaign.ImageUrl = campaignDto.ImageUrl;
                campaign.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                campaignDto.Id = campaign.Id;
                campaignDto.CreatedAt = campaign.CreatedAt;
                campaignDto.UpdatedAt = campaign.UpdatedAt;

                return new BaseResponseDto<CampaignDto>
                {
                    Success = true,
                    Data = campaignDto,
                    Message = "Campaign updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating campaign {CampaignId}", id);
                return new BaseResponseDto<CampaignDto>
                {
                    Success = false,
                    Message = "Error updating campaign"
                };
            }
        }

        public async Task<BaseResponseDto<bool>> DeleteCampaignAsync(int id)
        {
            try
            {
                var campaign = await _context.Campaigns
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

                if (campaign == null)
                {
                    return new BaseResponseDto<bool>
                    {
                        Success = false,
                        Message = "Campaign not found"
                    };
                }

                campaign.IsActive = false;
                campaign.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Campaign deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting campaign {CampaignId}", id);
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Error deleting campaign"
                };
            }
        }

        public async Task<BaseResponseDto<bool>> ActivateCampaignAsync(int id)
        {
            try
            {
                var campaign = await _context.Campaigns
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (campaign == null)
                {
                    return new BaseResponseDto<bool>
                    {
                        Success = false,
                        Message = "Campaign not found"
                    };
                }

                campaign.IsActive = true;
                campaign.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Campaign activated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating campaign {CampaignId}", id);
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Error activating campaign"
                };
            }
        }

        public async Task<BaseResponseDto<bool>> DeactivateCampaignAsync(int id)
        {
            try
            {
                var campaign = await _context.Campaigns
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (campaign == null)
                {
                    return new BaseResponseDto<bool>
                    {
                        Success = false,
                        Message = "Campaign not found"
                    };
                }

                campaign.IsActive = false;
                campaign.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Campaign deactivated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating campaign {CampaignId}", id);
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Error deactivating campaign"
                };
            }
        }
    }
}
