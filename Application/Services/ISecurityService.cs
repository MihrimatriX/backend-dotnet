using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface ISecurityService
    {
        Task<BaseResponseDto<SecurityDto>> GetSecurityInfoAsync(int userId);
        Task<BaseResponseDto<string>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<BaseResponseDto<string>> UpdateEmailAsync(int userId, UpdateEmailDto updateEmailDto);
        Task<BaseResponseDto<string>> EnableTwoFactorAsync(int userId, EnableTwoFactorDto enableTwoFactorDto);
        Task<BaseResponseDto<string>> DisableTwoFactorAsync(int userId, DisableTwoFactorDto disableTwoFactorDto);
        Task<BaseResponseDto<List<LoginHistoryDto>>> GetLoginHistoryAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<BaseResponseDto<SecuritySettingsDto>> GetSecuritySettingsAsync(int userId);
        Task<BaseResponseDto<SecuritySettingsDto>> UpdateSecuritySettingsAsync(int userId, SecuritySettingsDto securitySettingsDto);
        Task<BaseResponseDto<string>> LogoutAllDevicesAsync(int userId);
    }
}
