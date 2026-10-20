using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    /// <summary>§4.14 — her zaman çağıranın ayarları.</summary>
    [Route("api/[controller]")]
    [Authorize]
    public class SettingsController : ApiControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet("user")]
        public async Task<ActionResult<BaseResponseDto<UserSettingsDto>>> GetUserSettings() =>
            Respond(await _settingsService.GetUserSettingsAsync(CurrentUserId));

        [HttpPut("user")]
        public async Task<ActionResult<BaseResponseDto<UserSettingsDto>>> UpdateUserSettings([FromBody] UpdateUserSettingsDto updateUserSettingsDto) =>
            Respond(await _settingsService.UpdateUserSettingsAsync(CurrentUserId, updateUserSettingsDto));

        [HttpGet("privacy")]
        public async Task<ActionResult<BaseResponseDto<PrivacySettingsDto>>> GetPrivacySettings() =>
            Respond(await _settingsService.GetPrivacySettingsAsync(CurrentUserId));

        [HttpPut("privacy")]
        public async Task<ActionResult<BaseResponseDto<PrivacySettingsDto>>> UpdatePrivacySettings([FromBody] UpdatePrivacySettingsDto updatePrivacySettingsDto) =>
            Respond(await _settingsService.UpdatePrivacySettingsAsync(CurrentUserId, updatePrivacySettingsDto));

        [HttpPost("reset")]
        public async Task<ActionResult<BaseResponseDto<string>>> ResetToDefaults() =>
            Respond(await _settingsService.ResetToDefaultsAsync(CurrentUserId));

        [HttpGet("export")]
        public async Task<ActionResult<BaseResponseDto<string>>> ExportSettings() =>
            Respond(await _settingsService.ExportSettingsAsync(CurrentUserId));

        /// <summary>Gövde: export metni (JSON string) veya doğrudan aynı nesne.</summary>
        [HttpPost("import")]
        public async Task<ActionResult<BaseResponseDto<string>>> ImportSettings([FromBody] JsonElement body) =>
            Respond(await _settingsService.ImportSettingsAsync(CurrentUserId, body));
    }
}
