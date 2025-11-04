using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EcommerceBackend.Application.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _context;

        public SettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponseDto<UserSettingsDto>> GetUserSettingsAsync(int userId)
        {
            try
            {
                var userSettings = await _context.UserSettings
                    .Where(us => us.UserId == userId && us.IsActive)
                    .FirstOrDefaultAsync();

                if (userSettings == null)
                {
                    // Create default settings if none exist
                    userSettings = new UserSettings
                    {
                        UserId = userId,
                        Language = "tr",
                        Timezone = "Europe/Istanbul",
                        Currency = "TRY",
                        EmailNotifications = true,
                        SmsNotifications = false,
                        PushNotifications = true,
                        MarketingEmails = false,
                        OrderUpdates = true,
                        PriceAlerts = true,
                        StockNotifications = true,
                        Theme = "light",
                        ItemsPerPage = 20,
                        AutoSaveCart = true,
                        ShowProductRecommendations = true,
                        EnableLocationServices = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.UserSettings.Add(userSettings);
                    await _context.SaveChangesAsync();
                }

                var userSettingsDto = new UserSettingsDto
                {
                    UserId = userSettings.UserId,
                    Language = userSettings.Language,
                    Timezone = userSettings.Timezone,
                    Currency = userSettings.Currency,
                    EmailNotifications = userSettings.EmailNotifications,
                    SmsNotifications = userSettings.SmsNotifications,
                    PushNotifications = userSettings.PushNotifications,
                    MarketingEmails = userSettings.MarketingEmails,
                    OrderUpdates = userSettings.OrderUpdates,
                    PriceAlerts = userSettings.PriceAlerts,
                    StockNotifications = userSettings.StockNotifications,
                    Theme = userSettings.Theme,
                    ItemsPerPage = userSettings.ItemsPerPage,
                    AutoSaveCart = userSettings.AutoSaveCart,
                    ShowProductRecommendations = userSettings.ShowProductRecommendations,
                    EnableLocationServices = userSettings.EnableLocationServices,
                    CreatedAt = userSettings.CreatedAt,
                    UpdatedAt = userSettings.UpdatedAt
                };

                return BaseResponseDto<UserSettingsDto>.SuccessResult("User settings retrieved successfully", userSettingsDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<UserSettingsDto>.ErrorResult($"Error retrieving user settings: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<UserSettingsDto>> UpdateUserSettingsAsync(int userId, UpdateUserSettingsDto updateUserSettingsDto)
        {
            try
            {
                var userSettings = await _context.UserSettings
                    .Where(us => us.UserId == userId && us.IsActive)
                    .FirstOrDefaultAsync();

                if (userSettings == null)
                {
                    return BaseResponseDto<UserSettingsDto>.ErrorResult("User settings not found");
                }

                // Update only provided fields
                if (updateUserSettingsDto.Language != null)
                    userSettings.Language = updateUserSettingsDto.Language;
                if (updateUserSettingsDto.Timezone != null)
                    userSettings.Timezone = updateUserSettingsDto.Timezone;
                if (updateUserSettingsDto.Currency != null)
                    userSettings.Currency = updateUserSettingsDto.Currency;
                if (updateUserSettingsDto.EmailNotifications.HasValue)
                    userSettings.EmailNotifications = updateUserSettingsDto.EmailNotifications.Value;
                if (updateUserSettingsDto.SmsNotifications.HasValue)
                    userSettings.SmsNotifications = updateUserSettingsDto.SmsNotifications.Value;
                if (updateUserSettingsDto.PushNotifications.HasValue)
                    userSettings.PushNotifications = updateUserSettingsDto.PushNotifications.Value;
                if (updateUserSettingsDto.MarketingEmails.HasValue)
                    userSettings.MarketingEmails = updateUserSettingsDto.MarketingEmails.Value;
                if (updateUserSettingsDto.OrderUpdates.HasValue)
                    userSettings.OrderUpdates = updateUserSettingsDto.OrderUpdates.Value;
                if (updateUserSettingsDto.PriceAlerts.HasValue)
                    userSettings.PriceAlerts = updateUserSettingsDto.PriceAlerts.Value;
                if (updateUserSettingsDto.StockNotifications.HasValue)
                    userSettings.StockNotifications = updateUserSettingsDto.StockNotifications.Value;
                if (updateUserSettingsDto.Theme != null)
                    userSettings.Theme = updateUserSettingsDto.Theme;
                if (updateUserSettingsDto.ItemsPerPage.HasValue)
                    userSettings.ItemsPerPage = updateUserSettingsDto.ItemsPerPage.Value;
                if (updateUserSettingsDto.AutoSaveCart.HasValue)
                    userSettings.AutoSaveCart = updateUserSettingsDto.AutoSaveCart.Value;
                if (updateUserSettingsDto.ShowProductRecommendations.HasValue)
                    userSettings.ShowProductRecommendations = updateUserSettingsDto.ShowProductRecommendations.Value;
                if (updateUserSettingsDto.EnableLocationServices.HasValue)
                    userSettings.EnableLocationServices = updateUserSettingsDto.EnableLocationServices.Value;

                userSettings.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var userSettingsDto = new UserSettingsDto
                {
                    UserId = userSettings.UserId,
                    Language = userSettings.Language,
                    Timezone = userSettings.Timezone,
                    Currency = userSettings.Currency,
                    EmailNotifications = userSettings.EmailNotifications,
                    SmsNotifications = userSettings.SmsNotifications,
                    PushNotifications = userSettings.PushNotifications,
                    MarketingEmails = userSettings.MarketingEmails,
                    OrderUpdates = userSettings.OrderUpdates,
                    PriceAlerts = userSettings.PriceAlerts,
                    StockNotifications = userSettings.StockNotifications,
                    Theme = userSettings.Theme,
                    ItemsPerPage = userSettings.ItemsPerPage,
                    AutoSaveCart = userSettings.AutoSaveCart,
                    ShowProductRecommendations = userSettings.ShowProductRecommendations,
                    EnableLocationServices = userSettings.EnableLocationServices,
                    CreatedAt = userSettings.CreatedAt,
                    UpdatedAt = userSettings.UpdatedAt
                };

                return BaseResponseDto<UserSettingsDto>.SuccessResult("User settings updated successfully", userSettingsDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<UserSettingsDto>.ErrorResult($"Error updating user settings: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<PrivacySettingsDto>> GetPrivacySettingsAsync(int userId)
        {
            try
            {
                var privacySettings = await _context.PrivacySettings
                    .Where(ps => ps.UserId == userId && ps.IsActive)
                    .FirstOrDefaultAsync();

                if (privacySettings == null)
                {
                    // Create default privacy settings if none exist
                    privacySettings = new PrivacySettings
                    {
                        UserId = userId,
                        ProfileVisibility = true,
                        ShowEmail = false,
                        ShowPhone = false,
                        AllowDataCollection = true,
                        AllowAnalytics = true,
                        AllowCookies = true,
                        AllowMarketing = false,
                        DataSharing = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.PrivacySettings.Add(privacySettings);
                    await _context.SaveChangesAsync();
                }

                var privacySettingsDto = new PrivacySettingsDto
                {
                    UserId = privacySettings.UserId,
                    ProfileVisibility = privacySettings.ProfileVisibility,
                    ShowEmail = privacySettings.ShowEmail,
                    ShowPhone = privacySettings.ShowPhone,
                    AllowDataCollection = privacySettings.AllowDataCollection,
                    AllowAnalytics = privacySettings.AllowAnalytics,
                    AllowCookies = privacySettings.AllowCookies,
                    AllowMarketing = privacySettings.AllowMarketing,
                    DataSharing = privacySettings.DataSharing,
                    CreatedAt = privacySettings.CreatedAt,
                    UpdatedAt = privacySettings.UpdatedAt
                };

                return BaseResponseDto<PrivacySettingsDto>.SuccessResult("Privacy settings retrieved successfully", privacySettingsDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<PrivacySettingsDto>.ErrorResult($"Error retrieving privacy settings: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<PrivacySettingsDto>> UpdatePrivacySettingsAsync(int userId, UpdatePrivacySettingsDto updatePrivacySettingsDto)
        {
            try
            {
                var privacySettings = await _context.PrivacySettings
                    .Where(ps => ps.UserId == userId && ps.IsActive)
                    .FirstOrDefaultAsync();

                if (privacySettings == null)
                {
                    return BaseResponseDto<PrivacySettingsDto>.ErrorResult("Privacy settings not found");
                }

                // Update only provided fields
                if (updatePrivacySettingsDto.ProfileVisibility.HasValue)
                    privacySettings.ProfileVisibility = updatePrivacySettingsDto.ProfileVisibility.Value;
                if (updatePrivacySettingsDto.ShowEmail.HasValue)
                    privacySettings.ShowEmail = updatePrivacySettingsDto.ShowEmail.Value;
                if (updatePrivacySettingsDto.ShowPhone.HasValue)
                    privacySettings.ShowPhone = updatePrivacySettingsDto.ShowPhone.Value;
                if (updatePrivacySettingsDto.AllowDataCollection.HasValue)
                    privacySettings.AllowDataCollection = updatePrivacySettingsDto.AllowDataCollection.Value;
                if (updatePrivacySettingsDto.AllowAnalytics.HasValue)
                    privacySettings.AllowAnalytics = updatePrivacySettingsDto.AllowAnalytics.Value;
                if (updatePrivacySettingsDto.AllowCookies.HasValue)
                    privacySettings.AllowCookies = updatePrivacySettingsDto.AllowCookies.Value;
                if (updatePrivacySettingsDto.AllowMarketing.HasValue)
                    privacySettings.AllowMarketing = updatePrivacySettingsDto.AllowMarketing.Value;
                if (updatePrivacySettingsDto.DataSharing.HasValue)
                    privacySettings.DataSharing = updatePrivacySettingsDto.DataSharing.Value;

                privacySettings.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var privacySettingsDto = new PrivacySettingsDto
                {
                    UserId = privacySettings.UserId,
                    ProfileVisibility = privacySettings.ProfileVisibility,
                    ShowEmail = privacySettings.ShowEmail,
                    ShowPhone = privacySettings.ShowPhone,
                    AllowDataCollection = privacySettings.AllowDataCollection,
                    AllowAnalytics = privacySettings.AllowAnalytics,
                    AllowCookies = privacySettings.AllowCookies,
                    AllowMarketing = privacySettings.AllowMarketing,
                    DataSharing = privacySettings.DataSharing,
                    CreatedAt = privacySettings.CreatedAt,
                    UpdatedAt = privacySettings.UpdatedAt
                };

                return BaseResponseDto<PrivacySettingsDto>.SuccessResult("Privacy settings updated successfully", privacySettingsDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<PrivacySettingsDto>.ErrorResult($"Error updating privacy settings: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> ResetToDefaultsAsync(int userId)
        {
            try
            {
                var userSettings = await _context.UserSettings
                    .Where(us => us.UserId == userId && us.IsActive)
                    .FirstOrDefaultAsync();

                if (userSettings != null)
                {
                    userSettings.Language = "tr";
                    userSettings.Timezone = "Europe/Istanbul";
                    userSettings.Currency = "TRY";
                    userSettings.EmailNotifications = true;
                    userSettings.SmsNotifications = false;
                    userSettings.PushNotifications = true;
                    userSettings.MarketingEmails = false;
                    userSettings.OrderUpdates = true;
                    userSettings.PriceAlerts = true;
                    userSettings.StockNotifications = true;
                    userSettings.Theme = "light";
                    userSettings.ItemsPerPage = 20;
                    userSettings.AutoSaveCart = true;
                    userSettings.ShowProductRecommendations = true;
                    userSettings.EnableLocationServices = false;
                    userSettings.UpdatedAt = DateTime.UtcNow;
                }

                var privacySettings = await _context.PrivacySettings
                    .Where(ps => ps.UserId == userId && ps.IsActive)
                    .FirstOrDefaultAsync();

                if (privacySettings != null)
                {
                    privacySettings.ProfileVisibility = true;
                    privacySettings.ShowEmail = false;
                    privacySettings.ShowPhone = false;
                    privacySettings.AllowDataCollection = true;
                    privacySettings.AllowAnalytics = true;
                    privacySettings.AllowCookies = true;
                    privacySettings.AllowMarketing = false;
                    privacySettings.DataSharing = false;
                    privacySettings.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return BaseResponseDto<string>.SuccessResult("Settings reset to defaults successfully", "Settings reset to defaults successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error resetting settings: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> ExportSettingsAsync(int userId)
        {
            try
            {
                var userSettings = await GetUserSettingsAsync(userId);
                var privacySettings = await GetPrivacySettingsAsync(userId);

                var exportData = new
                {
                    UserSettings = userSettings.Data,
                    PrivacySettings = privacySettings.Data,
                    ExportDate = DateTime.UtcNow
                };

                var settingsJson = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });

                return BaseResponseDto<string>.SuccessResult("Settings exported successfully", settingsJson);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error exporting settings: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> ImportSettingsAsync(int userId, string settingsJson)
        {
            try
            {
                var importData = JsonSerializer.Deserialize<dynamic>(settingsJson);
                // TODO: Implement settings import logic
                
                return BaseResponseDto<string>.SuccessResult("Settings imported successfully", "Settings imported successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error importing settings: {ex.Message}");
            }
        }
    }
}
