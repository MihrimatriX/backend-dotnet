using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    public class CampaignController : ApiControllerBase
    {
        private readonly ICampaignService _campaignService;

        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<List<CampaignDto>>>> GetCampaigns() =>
            Respond(await _campaignService.GetAllCampaignsAsync());

        [HttpGet("active")]
        public async Task<ActionResult<BaseResponseDto<List<CampaignDto>>>> GetActiveCampaigns() =>
            Respond(await _campaignService.GetActiveCampaignsAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseDto<CampaignDto>>> GetCampaign(int id) =>
            Respond(await _campaignService.GetCampaignByIdAsync(id));

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<CampaignDto>>> CreateCampaign([FromBody] CreateCampaignDto createCampaignDto) =>
            RespondCreated(await _campaignService.CreateCampaignAsync(createCampaignDto));

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<CampaignDto>>> UpdateCampaign(int id, [FromBody] UpdateCampaignDto updateCampaignDto) =>
            Respond(await _campaignService.UpdateCampaignAsync(id, updateCampaignDto));

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteCampaign(int id) =>
            Respond(await _campaignService.DeleteCampaignAsync(id));
    }
}
