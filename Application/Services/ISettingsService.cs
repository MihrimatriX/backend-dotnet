using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface ISettingsService
    {
        Task<BaseResponseDto<UserSettingsDto>> GetUserSettingsAsync(int userId);
        Task<BaseResponseDto<UserSettingsDto>> UpdateUserSettingsAsync(int userId, UpdateUserSettingsDto updateUserSettingsDto);
        Task<BaseResponseDto<PrivacySettingsDto>> GetPrivacySettingsAsync(int userId);
        Task<BaseResponseDto<PrivacySettingsDto>> UpdatePrivacySettingsAsync(int userId, UpdatePrivacySettingsDto updatePrivacySettingsDto);
        Task<BaseResponseDto<string>> ResetToDefaultsAsync(int userId);
        Task<BaseResponseDto<string>> ExportSettingsAsync(int userId);
        Task<BaseResponseDto<string>> ImportSettingsAsync(int userId, string settingsJson);
    }
}
