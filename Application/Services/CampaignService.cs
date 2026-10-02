using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ApplicationDbContext _context;

        public CampaignService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Aktif kampanyalar, en yeni önce.</summary>
        public async Task<BaseResponseDto<List<CampaignDto>>> GetAllCampaignsAsync()
        {
            var campaigns = await _context.Campaigns
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ThenByDescending(c => c.Id)
                .ToListAsync();

            return BaseResponseDto<List<CampaignDto>>.SuccessResult(
                "Campaigns retrieved successfully",
                campaigns.Select(ToDto).ToList());
        }

        /// <summary>Aktif ve <c>startDate ≤ şimdi (UTC) ≤ endDate</c>.</summary>
        public async Task<BaseResponseDto<List<CampaignDto>>> GetActiveCampaignsAsync()
        {
            var now = DateTime.UtcNow;
            var campaigns = await _context.Campaigns
                .AsNoTracking()
                .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now)
                .OrderByDescending(c => c.CreatedAt)
                .ThenByDescending(c => c.Id)
                .ToListAsync();

            return BaseResponseDto<List<CampaignDto>>.SuccessResult(
                "Active campaigns retrieved successfully",
                campaigns.Select(ToDto).ToList());
        }

        public async Task<BaseResponseDto<CampaignDto>> GetCampaignByIdAsync(int id)
        {
            var campaign = await _context.Campaigns
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            return campaign == null
                ? CampaignNotFound<CampaignDto>()
                : BaseResponseDto<CampaignDto>.SuccessResult("Campaign retrieved successfully", ToDto(campaign));
        }

        public async Task<BaseResponseDto<CampaignDto>> CreateCampaignAsync(CreateCampaignDto dto)
        {
            var (startDate, endDate) = (dto.StartDate!.Value, dto.EndDate!.Value);
            if (endDate < startDate)
                return InvalidDateRange();

            var campaign = new Campaign
            {
                Title = dto.Title.Trim(),
                Subtitle = dto.Subtitle,
                Description = dto.Description,
                Discount = dto.Discount,
                ImageUrl = dto.ImageUrl,
                BackgroundColor = dto.BackgroundColor,
                TimeLeft = dto.TimeLeft,
                ButtonText = dto.ButtonText,
                ButtonHref = dto.ButtonHref,
                StartDate = startDate,
                EndDate = endDate,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            return BaseResponseDto<CampaignDto>.SuccessResult("Campaign created successfully", ToDto(campaign));
        }

        /// <summary>Pasif kampanyalar da güncellenebilir; <c>isActive</c> gönderilmezse değişmez.</summary>
        public async Task<BaseResponseDto<CampaignDto>> UpdateCampaignAsync(int id, UpdateCampaignDto dto)
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == id);
            if (campaign == null)
                return CampaignNotFound<CampaignDto>();

            var (startDate, endDate) = (dto.StartDate!.Value, dto.EndDate!.Value);
            if (endDate < startDate)
                return InvalidDateRange();

            campaign.Title = dto.Title.Trim();
            campaign.Subtitle = dto.Subtitle;
            campaign.Description = dto.Description;
            campaign.Discount = dto.Discount;
            campaign.ImageUrl = dto.ImageUrl;
            campaign.BackgroundColor = dto.BackgroundColor;
            campaign.TimeLeft = dto.TimeLeft;
            campaign.ButtonText = dto.ButtonText;
            campaign.ButtonHref = dto.ButtonHref;
            campaign.StartDate = startDate;
            campaign.EndDate = endDate;
            if (dto.IsActive is { } isActive)
                campaign.IsActive = isActive;
            campaign.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BaseResponseDto<CampaignDto>.SuccessResult("Campaign updated successfully", ToDto(campaign));
        }

        public async Task<BaseResponseDto<string>> DeleteCampaignAsync(int id)
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
            if (campaign == null)
                return CampaignNotFound<string>();

            campaign.IsActive = false;
            campaign.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<string>.SuccessResult("Campaign deleted successfully", "Campaign deleted successfully");
        }

        private static BaseResponseDto<T> CampaignNotFound<T>() =>
            BaseResponseDto<T>.NotFound("Campaign not found", ErrorCodes.CampaignNotFound);

        private static BaseResponseDto<CampaignDto> InvalidDateRange() =>
            BaseResponseDto<CampaignDto>.Fail("End date must be on or after the start date", ErrorCodes.InvalidDateRange);

        private static CampaignDto ToDto(Campaign c) => new()
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
        };
    }
}
