using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;
using System.Security.Claims;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityService _securityService;

        public SecurityController(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        [HttpGet("info")]
        public async Task<ActionResult<BaseResponseDto<SecurityDto>>> GetSecurityInfo()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _securityService.GetSecurityInfoAsync(currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<SecurityDto>.ErrorResult($"Error retrieving security information: {ex.Message}"));
            }
        }

        [HttpPost("change-password")]
        public async Task<ActionResult<BaseResponseDto<string>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _securityService.ChangePasswordAsync(currentUserId, changePasswordDto);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error changing password: {ex.Message}"));
            }
        }

        [HttpPost("update-email")]
        public async Task<ActionResult<BaseResponseDto<string>>> UpdateEmail([FromBody] UpdateEmailDto updateEmailDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _securityService.UpdateEmailAsync(currentUserId, updateEmailDto);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error updating email: {ex.Message}"));
            }
        }

        [HttpPost("enable-2fa")]
        public async Task<ActionResult<BaseResponseDto<string>>> EnableTwoFactor([FromBody] EnableTwoFactorDto enableTwoFactorDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _securityService.EnableTwoFactorAsync(currentUserId, enableTwoFactorDto);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error enabling two-factor authentication: {ex.Message}"));
            }
        }

        [HttpPost("disable-2fa")]
        public async Task<ActionResult<BaseResponseDto<string>>> DisableTwoFactor([FromBody] DisableTwoFactorDto disableTwoFactorDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _securityService.DisableTwoFactorAsync(currentUserId, disableTwoFactorDto);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error disabling two-factor authentication: {ex.Message}"));
            }
        }

        [HttpGet("login-history")]
        public async Task<ActionResult<BaseResponseDto<List<LoginHistoryDto>>>> GetLoginHistory(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _securityService.GetLoginHistoryAsync(currentUserId, pageNumber, pageSize);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<List<LoginHistoryDto>>.ErrorResult($"Error retrieving login history: {ex.Message}"));
            }
        }

        [HttpGet("settings")]
        public async Task<ActionResult<BaseResponseDto<SecuritySettingsDto>>> GetSecuritySettings()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _securityService.GetSecuritySettingsAsync(currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<SecuritySettingsDto>.ErrorResult($"Error retrieving security settings: {ex.Message}"));
            }
        }

        [HttpPut("settings")]
        public async Task<ActionResult<BaseResponseDto<SecuritySettingsDto>>> UpdateSecuritySettings([FromBody] SecuritySettingsDto securitySettingsDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _securityService.UpdateSecuritySettingsAsync(currentUserId, securitySettingsDto);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<SecuritySettingsDto>.ErrorResult($"Error updating security settings: {ex.Message}"));
            }
        }

        [HttpPost("logout-all-devices")]
        public async Task<ActionResult<BaseResponseDto<string>>> LogoutAllDevices()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _securityService.LogoutAllDevicesAsync(currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error logging out all devices: {ex.Message}"));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid user ID");
        }
    }
}
