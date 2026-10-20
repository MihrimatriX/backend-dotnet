using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface ISecurityService
    {
        Task<BaseResponseDto<SecurityDto>> GetSecurityInfoAsync(int userId);
        /// <param name="currentTokenId">Çağıran token'ın <c>jti</c> değeri; şifre değişince bu token geçerli kalır.</param>
        Task<BaseResponseDto<string>> ChangePasswordAsync(int userId, string? currentTokenId, ChangePasswordDto changePasswordDto);
        Task<BaseResponseDto<string>> UpdateEmailAsync(int userId, UpdateEmailDto updateEmailDto);
        Task<BaseResponseDto<string>> EnableTwoFactorAsync(int userId, EnableTwoFactorDto enableTwoFactorDto);
        Task<BaseResponseDto<string>> DisableTwoFactorAsync(int userId, DisableTwoFactorDto disableTwoFactorDto);
        Task<BaseResponseDto<List<LoginHistoryDto>>> GetLoginHistoryAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<BaseResponseDto<SecuritySettingsDto>> GetSecuritySettingsAsync(int userId);
        Task<BaseResponseDto<SecuritySettingsDto>> UpdateSecuritySettingsAsync(int userId, SecuritySettingsDto securitySettingsDto);
        /// <param name="currentTokenId">Çağıran token'ın <c>jti</c> değeri; iptalden muaf tutulur (§5.6).</param>
        Task<BaseResponseDto<string>> LogoutAllDevicesAsync(int userId, string? currentTokenId);
    }
}
