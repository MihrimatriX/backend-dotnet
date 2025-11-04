using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CampaignController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<List<CampaignDto>>>> GetCampaigns()
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
                        Discount = c.Discount,
                        ImageUrl = c.ImageUrl,
                        BackgroundColor = c.BackgroundColor,
                        TimeLeft = c.TimeLeft,
                        ButtonText = c.ButtonText,
                        ButtonHref = c.ButtonHref,
                        IsActive = c.IsActive,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(BaseResponseDto<List<CampaignDto>>.SuccessResult("Campaigns retrieved successfully", campaigns));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<List<CampaignDto>>.ErrorResult($"Error retrieving campaigns: {ex.Message}"));
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<BaseResponseDto<List<CampaignDto>>>> GetActiveCampaigns()
        {
            try
            {
                var now = DateTime.UtcNow;
                var campaigns = await _context.Campaigns
                    .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CampaignDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Subtitle = c.Subtitle,
                        Description = c.Description,
                        Discount = c.Discount,
                        ImageUrl = c.ImageUrl,
                        BackgroundColor = c.BackgroundColor,
                        TimeLeft = c.TimeLeft,
                        ButtonText = c.ButtonText,
                        ButtonHref = c.ButtonHref,
                        IsActive = c.IsActive,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(BaseResponseDto<List<CampaignDto>>.SuccessResult("Active campaigns retrieved successfully", campaigns));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<List<CampaignDto>>.ErrorResult($"Error retrieving active campaigns: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseDto<CampaignDto>>> GetCampaign(int id)
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
                        Discount = c.Discount,
                        ImageUrl = c.ImageUrl,
                        BackgroundColor = c.BackgroundColor,
                        TimeLeft = c.TimeLeft,
                        ButtonText = c.ButtonText,
                        ButtonHref = c.ButtonHref,
                        IsActive = c.IsActive,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (campaign == null)
                {
                    return NotFound(BaseResponseDto<CampaignDto>.ErrorResult("Campaign not found"));
                }

                return Ok(BaseResponseDto<CampaignDto>.SuccessResult("Campaign retrieved successfully", campaign));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<CampaignDto>.ErrorResult($"Error retrieving campaign: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponseDto<CampaignDto>>> CreateCampaign([FromBody] CreateCampaignDto createCampaignDto)
        {
            try
            {
                var campaign = new Campaign
                {
                    Title = createCampaignDto.Title,
                    Subtitle = createCampaignDto.Subtitle,
                    Description = createCampaignDto.Description,
                    Discount = createCampaignDto.Discount,
                    ImageUrl = createCampaignDto.ImageUrl,
                    BackgroundColor = createCampaignDto.BackgroundColor,
                    TimeLeft = createCampaignDto.TimeLeft,
                    ButtonText = createCampaignDto.ButtonText,
                    ButtonHref = createCampaignDto.ButtonHref,
                    StartDate = createCampaignDto.StartDate,
                    EndDate = createCampaignDto.EndDate,
                    IsActive = createCampaignDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Campaigns.Add(campaign);
                await _context.SaveChangesAsync();

                var campaignDto = new CampaignDto
                {
                    Id = campaign.Id,
                    Title = campaign.Title,
                    Subtitle = campaign.Subtitle,
                    Description = campaign.Description,
                    Discount = campaign.Discount,
                    ImageUrl = campaign.ImageUrl,
                    BackgroundColor = campaign.BackgroundColor,
                    TimeLeft = campaign.TimeLeft,
                    ButtonText = campaign.ButtonText,
                    ButtonHref = campaign.ButtonHref,
                    IsActive = campaign.IsActive,
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    CreatedAt = campaign.CreatedAt,
                    UpdatedAt = campaign.UpdatedAt
                };

                return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, 
                    BaseResponseDto<CampaignDto>.SuccessResult("Campaign created successfully", campaignDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<CampaignDto>.ErrorResult($"Error creating campaign: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponseDto<CampaignDto>>> UpdateCampaign(int id, [FromBody] UpdateCampaignDto updateCampaignDto)
        {
            try
            {
                var campaign = await _context.Campaigns.FindAsync(id);
                if (campaign == null)
                {
                    return NotFound(BaseResponseDto<CampaignDto>.ErrorResult("Campaign not found"));
                }

                campaign.Title = updateCampaignDto.Title;
                campaign.Subtitle = updateCampaignDto.Subtitle;
                campaign.Description = updateCampaignDto.Description;
                campaign.Discount = updateCampaignDto.Discount;
                campaign.ImageUrl = updateCampaignDto.ImageUrl;
                campaign.BackgroundColor = updateCampaignDto.BackgroundColor;
                campaign.TimeLeft = updateCampaignDto.TimeLeft;
                campaign.ButtonText = updateCampaignDto.ButtonText;
                campaign.ButtonHref = updateCampaignDto.ButtonHref;
                campaign.StartDate = updateCampaignDto.StartDate;
                campaign.EndDate = updateCampaignDto.EndDate;
                campaign.IsActive = updateCampaignDto.IsActive;
                campaign.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var campaignDto = new CampaignDto
                {
                    Id = campaign.Id,
                    Title = campaign.Title,
                    Subtitle = campaign.Subtitle,
                    Description = campaign.Description,
                    Discount = campaign.Discount,
                    ImageUrl = campaign.ImageUrl,
                    BackgroundColor = campaign.BackgroundColor,
                    TimeLeft = campaign.TimeLeft,
                    ButtonText = campaign.ButtonText,
                    ButtonHref = campaign.ButtonHref,
                    IsActive = campaign.IsActive,
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    CreatedAt = campaign.CreatedAt,
                    UpdatedAt = campaign.UpdatedAt
                };

                return Ok(BaseResponseDto<CampaignDto>.SuccessResult("Campaign updated successfully", campaignDto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<CampaignDto>.ErrorResult($"Error updating campaign: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteCampaign(int id)
        {
            try
            {
                var campaign = await _context.Campaigns.FindAsync(id);
                if (campaign == null)
                {
                    return NotFound(BaseResponseDto<string>.ErrorResult("Campaign not found"));
                }

                campaign.IsActive = false;
                campaign.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(BaseResponseDto<string>.SuccessResult("Campaign deleted successfully", "Campaign deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error deleting campaign: {ex.Message}"));
            }
        }
    }
}
