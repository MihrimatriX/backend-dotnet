using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Serialization;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    /// <summary>§4.14 — her zaman çağıranın ayarları; kayıt yoksa entity varsayılanlarıyla oluşturulur.</summary>
    public class SettingsService : ISettingsService
    {
        private const string ResetMessage = "Settings reset to defaults successfully";
        private const string ImportMessage = "Settings imported successfully";
        private const string InvalidImportMessage = "Invalid settings data";

        private readonly ApplicationDbContext _context;

        public SettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponseDto<UserSettingsDto>> GetUserSettingsAsync(int userId) =>
            BaseResponseDto<UserSettingsDto>.SuccessResult("User settings retrieved successfully", ToDto(await GetOrCreateUserSettingsAsync(userId)));

        /// <summary>Upsert + kısmi güncelleme: gönderilmeyen alan değişmez.</summary>
        public async Task<BaseResponseDto<UserSettingsDto>> UpdateUserSettingsAsync(int userId, UpdateUserSettingsDto updateUserSettingsDto)
        {
            var settings = await GetOrCreateUserSettingsAsync(userId);
            Apply(settings, updateUserSettingsDto);
            await _context.SaveChangesAsync();
            return BaseResponseDto<UserSettingsDto>.SuccessResult("User settings updated successfully", ToDto(settings));
        }

        public async Task<BaseResponseDto<PrivacySettingsDto>> GetPrivacySettingsAsync(int userId) =>
            BaseResponseDto<PrivacySettingsDto>.SuccessResult("Privacy settings retrieved successfully", ToDto(await GetOrCreatePrivacySettingsAsync(userId)));

        /// <summary>Upsert + kısmi güncelleme: gönderilmeyen alan değişmez.</summary>
        public async Task<BaseResponseDto<PrivacySettingsDto>> UpdatePrivacySettingsAsync(int userId, UpdatePrivacySettingsDto updatePrivacySettingsDto)
        {
            var settings = await GetOrCreatePrivacySettingsAsync(userId);
            Apply(settings, updatePrivacySettingsDto);
            await _context.SaveChangesAsync();
            return BaseResponseDto<PrivacySettingsDto>.SuccessResult("Privacy settings updated successfully", ToDto(settings));
        }

        public async Task<BaseResponseDto<string>> ResetToDefaultsAsync(int userId)
        {
            var user = await GetOrCreateUserSettingsAsync(userId);
            var privacy = await GetOrCreatePrivacySettingsAsync(userId);

            // Varsayılanlar entity ilk değerlerinden gelir (tek kaynak).
            Apply(user, FromEntity(new UserSettings()));
            Apply(privacy, FromEntity(new PrivacySettings()));

            await _context.SaveChangesAsync();
            return BaseResponseDto<string>.SuccessResult(ResetMessage, ResetMessage);
        }

        /// <summary>data = JSON metni; ApiJson ile camelCase ve UTC <c>Z</c> tarihler.</summary>
        public async Task<BaseResponseDto<string>> ExportSettingsAsync(int userId)
        {
            var export = new
            {
                UserSettings = ToDto(await GetOrCreateUserSettingsAsync(userId)),
                PrivacySettings = ToDto(await GetOrCreatePrivacySettingsAsync(userId)),
                ExportDate = DateTime.UtcNow
            };

            return BaseResponseDto<string>.SuccessResult("Settings exported successfully", JsonSerializer.Serialize(export, ApiJson.Options));
        }

        /// <summary>
        /// Gövde: export metnini içeren JSON string veya doğrudan aynı nesne. Bulunan bölümler kısmi güncelleme
        /// olarak uygulanır; okunamayan/geçersiz veri → 400 INVALID_IMPORT.
        /// </summary>
        public async Task<BaseResponseDto<string>> ImportSettingsAsync(int userId, JsonElement body)
        {
            var data = ParseImport(body);
            if (data == null || (data.UserSettings == null && data.PrivacySettings == null)
                || (data.UserSettings != null && !IsValid(data.UserSettings))
                || (data.PrivacySettings != null && !IsValid(data.PrivacySettings)))
                return BaseResponseDto<string>.Fail(InvalidImportMessage, ErrorCodes.InvalidImport);

            if (data.UserSettings != null)
                Apply(await GetOrCreateUserSettingsAsync(userId), data.UserSettings);
            if (data.PrivacySettings != null)
                Apply(await GetOrCreatePrivacySettingsAsync(userId), data.PrivacySettings);

            await _context.SaveChangesAsync();
            return BaseResponseDto<string>.SuccessResult(ImportMessage, ImportMessage);
        }

        private static ImportData? ParseImport(JsonElement body)
        {
            try
            {
                if (body.ValueKind == JsonValueKind.String)
                {
                    using var doc = JsonDocument.Parse(body.GetString()!);
                    return doc.RootElement.ValueKind == JsonValueKind.Object
                        ? doc.RootElement.Deserialize<ImportData>(ApiJson.Options)
                        : null;
                }

                return body.ValueKind == JsonValueKind.Object ? body.Deserialize<ImportData>(ApiJson.Options) : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static bool IsValid(object dto) =>
            Validator.TryValidateObject(dto, new ValidationContext(dto), null, validateAllProperties: true);

        private async Task<UserSettings> GetOrCreateUserSettingsAsync(int userId)
        {
            var settings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
            if (settings != null)
                return settings;

            settings = new UserSettings { UserId = userId };
            _context.UserSettings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        private async Task<PrivacySettings> GetOrCreatePrivacySettingsAsync(int userId)
        {
            var settings = await _context.PrivacySettings.FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
            if (settings != null)
                return settings;

            settings = new PrivacySettings { UserId = userId };
            _context.PrivacySettings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        private static void Apply(UserSettings s, UpdateUserSettingsDto d)
        {
            s.Language = d.Language ?? s.Language;
            s.Timezone = d.Timezone ?? s.Timezone;
            s.Currency = d.Currency ?? s.Currency;
            s.EmailNotifications = d.EmailNotifications ?? s.EmailNotifications;
            s.SmsNotifications = d.SmsNotifications ?? s.SmsNotifications;
            s.PushNotifications = d.PushNotifications ?? s.PushNotifications;
            s.MarketingEmails = d.MarketingEmails ?? s.MarketingEmails;
            s.OrderUpdates = d.OrderUpdates ?? s.OrderUpdates;
            s.PriceAlerts = d.PriceAlerts ?? s.PriceAlerts;
            s.StockNotifications = d.StockNotifications ?? s.StockNotifications;
            s.Theme = d.Theme ?? s.Theme;
            s.ItemsPerPage = d.ItemsPerPage ?? s.ItemsPerPage;
            s.AutoSaveCart = d.AutoSaveCart ?? s.AutoSaveCart;
            s.ShowProductRecommendations = d.ShowProductRecommendations ?? s.ShowProductRecommendations;
            s.EnableLocationServices = d.EnableLocationServices ?? s.EnableLocationServices;
            s.UpdatedAt = DateTime.UtcNow;
        }

        private static void Apply(PrivacySettings s, UpdatePrivacySettingsDto d)
        {
            s.ProfileVisibility = d.ProfileVisibility ?? s.ProfileVisibility;
            s.ShowEmail = d.ShowEmail ?? s.ShowEmail;
            s.ShowPhone = d.ShowPhone ?? s.ShowPhone;
            s.AllowDataCollection = d.AllowDataCollection ?? s.AllowDataCollection;
            s.AllowAnalytics = d.AllowAnalytics ?? s.AllowAnalytics;
            s.AllowCookies = d.AllowCookies ?? s.AllowCookies;
            s.AllowMarketing = d.AllowMarketing ?? s.AllowMarketing;
            s.DataSharing = d.DataSharing ?? s.DataSharing;
            s.UpdatedAt = DateTime.UtcNow;
        }

        private static UpdateUserSettingsDto FromEntity(UserSettings s) => new()
        {
            Language = s.Language,
            Timezone = s.Timezone,
            Currency = s.Currency,
            EmailNotifications = s.EmailNotifications,
            SmsNotifications = s.SmsNotifications,
            PushNotifications = s.PushNotifications,
            MarketingEmails = s.MarketingEmails,
            OrderUpdates = s.OrderUpdates,
            PriceAlerts = s.PriceAlerts,
            StockNotifications = s.StockNotifications,
            Theme = s.Theme,
            ItemsPerPage = s.ItemsPerPage,
            AutoSaveCart = s.AutoSaveCart,
            ShowProductRecommendations = s.ShowProductRecommendations,
            EnableLocationServices = s.EnableLocationServices
        };

        private static UpdatePrivacySettingsDto FromEntity(PrivacySettings s) => new()
        {
            ProfileVisibility = s.ProfileVisibility,
            ShowEmail = s.ShowEmail,
            ShowPhone = s.ShowPhone,
            AllowDataCollection = s.AllowDataCollection,
            AllowAnalytics = s.AllowAnalytics,
            AllowCookies = s.AllowCookies,
            AllowMarketing = s.AllowMarketing,
            DataSharing = s.DataSharing
        };

        public static UserSettingsDto ToDto(UserSettings s) => new()
        {
            UserId = s.UserId,
            Language = s.Language,
            Timezone = s.Timezone,
            Currency = s.Currency,
            EmailNotifications = s.EmailNotifications,
            SmsNotifications = s.SmsNotifications,
            PushNotifications = s.PushNotifications,
            MarketingEmails = s.MarketingEmails,
            OrderUpdates = s.OrderUpdates,
            PriceAlerts = s.PriceAlerts,
            StockNotifications = s.StockNotifications,
            Theme = s.Theme,
            ItemsPerPage = s.ItemsPerPage,
            AutoSaveCart = s.AutoSaveCart,
            ShowProductRecommendations = s.ShowProductRecommendations,
            EnableLocationServices = s.EnableLocationServices,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };

        public static PrivacySettingsDto ToDto(PrivacySettings s) => new()
        {
            UserId = s.UserId,
            ProfileVisibility = s.ProfileVisibility,
            ShowEmail = s.ShowEmail,
            ShowPhone = s.ShowPhone,
            AllowDataCollection = s.AllowDataCollection,
            AllowAnalytics = s.AllowAnalytics,
            AllowCookies = s.AllowCookies,
            AllowMarketing = s.AllowMarketing,
            DataSharing = s.DataSharing,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };

        /// <summary>Export biçimi; fazladan alanlar (userId, createdAt, exportDate) yok sayılır.</summary>
        private sealed class ImportData
        {
            public UpdateUserSettingsDto? UserSettings { get; set; }
            public UpdatePrivacySettingsDto? PrivacySettings { get; set; }
        }
    }
}
