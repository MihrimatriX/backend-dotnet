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
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet("user")]
        public async Task<ActionResult<BaseResponseDto<UserSettingsDto>>> GetUserSettings()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _settingsService.GetUserSettingsAsync(currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<UserSettingsDto>.ErrorResult($"Error retrieving user settings: {ex.Message}"));
            }
        }

        [HttpPut("user")]
        public async Task<ActionResult<BaseResponseDto<UserSettingsDto>>> UpdateUserSettings([FromBody] UpdateUserSettingsDto updateUserSettingsDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _settingsService.UpdateUserSettingsAsync(currentUserId, updateUserSettingsDto);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<UserSettingsDto>.ErrorResult($"Error updating user settings: {ex.Message}"));
            }
        }

        [HttpGet("privacy")]
        public async Task<ActionResult<BaseResponseDto<PrivacySettingsDto>>> GetPrivacySettings()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _settingsService.GetPrivacySettingsAsync(currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<PrivacySettingsDto>.ErrorResult($"Error retrieving privacy settings: {ex.Message}"));
            }
        }

        [HttpPut("privacy")]
        public async Task<ActionResult<BaseResponseDto<PrivacySettingsDto>>> UpdatePrivacySettings([FromBody] UpdatePrivacySettingsDto updatePrivacySettingsDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _settingsService.UpdatePrivacySettingsAsync(currentUserId, updatePrivacySettingsDto);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<PrivacySettingsDto>.ErrorResult($"Error updating privacy settings: {ex.Message}"));
            }
        }

        [HttpPost("reset")]
        public async Task<ActionResult<BaseResponseDto<string>>> ResetToDefaults()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _settingsService.ResetToDefaultsAsync(currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error resetting settings: {ex.Message}"));
            }
        }

        [HttpGet("export")]
        public async Task<ActionResult<BaseResponseDto<string>>> ExportSettings()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _settingsService.ExportSettingsAsync(currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error exporting settings: {ex.Message}"));
            }
        }

        [HttpPost("import")]
        public async Task<ActionResult<BaseResponseDto<string>>> ImportSettings([FromBody] string settingsJson)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _settingsService.ImportSettingsAsync(currentUserId, settingsJson);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error importing settings: {ex.Message}"));
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
