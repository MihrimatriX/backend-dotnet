using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Security;
using EcommerceBackend.Application.Services;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class SecurityController : ApiControllerBase
    {
        private readonly ISecurityService _securityService;

        public SecurityController(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        [HttpGet("info")]
        public async Task<ActionResult<BaseResponseDto<SecurityDto>>> GetSecurityInfo() =>
            Respond(await _securityService.GetSecurityInfoAsync(CurrentUserId));

        [HttpPost("change-password")]
        public async Task<ActionResult<BaseResponseDto<string>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto) =>
            Respond(await _securityService.ChangePasswordAsync(CurrentUserId, User.GetTokenId(), changePasswordDto));

        [HttpPost("update-email")]
        public async Task<ActionResult<BaseResponseDto<string>>> UpdateEmail([FromBody] UpdateEmailDto updateEmailDto) =>
            Respond(await _securityService.UpdateEmailAsync(CurrentUserId, updateEmailDto));

        [HttpPost("enable-2fa")]
        public async Task<ActionResult<BaseResponseDto<string>>> EnableTwoFactor([FromBody] EnableTwoFactorDto enableTwoFactorDto) =>
            Respond(await _securityService.EnableTwoFactorAsync(CurrentUserId, enableTwoFactorDto));

        [HttpPost("disable-2fa")]
        public async Task<ActionResult<BaseResponseDto<string>>> DisableTwoFactor([FromBody] DisableTwoFactorDto disableTwoFactorDto) =>
            Respond(await _securityService.DisableTwoFactorAsync(CurrentUserId, disableTwoFactorDto));

        [HttpGet("login-history")]
        public async Task<ActionResult<BaseResponseDto<List<LoginHistoryDto>>>> GetLoginHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
            Respond(await _securityService.GetLoginHistoryAsync(CurrentUserId, pageNumber, pageSize));

        [HttpGet("settings")]
        public async Task<ActionResult<BaseResponseDto<SecuritySettingsDto>>> GetSecuritySettings() =>
            Respond(await _securityService.GetSecuritySettingsAsync(CurrentUserId));

        [HttpPut("settings")]
        public async Task<ActionResult<BaseResponseDto<SecuritySettingsDto>>> UpdateSecuritySettings([FromBody] SecuritySettingsDto securitySettingsDto) =>
            Respond(await _securityService.UpdateSecuritySettingsAsync(CurrentUserId, securitySettingsDto));

        [HttpPost("logout-all-devices")]
        public async Task<ActionResult<BaseResponseDto<string>>> LogoutAllDevices() =>
            Respond(await _securityService.LogoutAllDevicesAsync(CurrentUserId, User.GetTokenId()));
    }
}
