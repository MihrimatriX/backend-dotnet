using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Services;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    public class HelpSupportController : ApiControllerBase
    {
        private readonly IHelpSupportService _helpSupportService;

        public HelpSupportController(IHelpSupportService helpSupportService)
        {
            _helpSupportService = helpSupportService;
        }

        [HttpGet("articles")]
        public async Task<ActionResult<BaseResponseDto<List<HelpArticleDto>>>> GetHelpArticles(
            [FromQuery] string? category = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
            Respond(await _helpSupportService.GetHelpArticlesAsync(category, pageNumber, pageSize));

        [HttpGet("articles/{articleId}")]
        public async Task<ActionResult<BaseResponseDto<HelpArticleDto>>> GetHelpArticle(int articleId) =>
            Respond(await _helpSupportService.GetHelpArticleByIdAsync(articleId));

        [HttpPost("articles")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<HelpArticleDto>>> CreateHelpArticle([FromBody] CreateHelpArticleDto createHelpArticleDto) =>
            RespondCreated(await _helpSupportService.CreateHelpArticleAsync(createHelpArticleDto));

        [HttpGet("faqs")]
        public async Task<ActionResult<BaseResponseDto<List<FaqDto>>>> GetFaqs(
            [FromQuery] string? category = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
            Respond(await _helpSupportService.GetFaqsAsync(category, pageNumber, pageSize));

        [HttpPost("contact")]
        public async Task<ActionResult<BaseResponseDto<string>>> SubmitContactForm([FromBody] ContactFormDto contactFormDto) =>
            Respond(await _helpSupportService.SubmitContactFormAsync(contactFormDto));

        [HttpGet("tickets/admin")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<List<SupportTicketDto>>>> GetAllSupportTicketsForAdmin(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20) =>
            Respond(await _helpSupportService.GetAllSupportTicketsAdminAsync(pageNumber, pageSize));

        [HttpGet("tickets")]
        [Authorize]
        public async Task<ActionResult<BaseResponseDto<List<SupportTicketDto>>>> GetUserSupportTickets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
            Respond(await _helpSupportService.GetUserSupportTicketsAsync(CurrentUserId, pageNumber, pageSize));

        [HttpPost("tickets")]
        [Authorize]
        public async Task<ActionResult<BaseResponseDto<SupportTicketDto>>> CreateSupportTicket([FromBody] CreateSupportTicketDto createSupportTicketDto) =>
            RespondCreated(await _helpSupportService.CreateSupportTicketAsync(CurrentUserId, createSupportTicketDto));
    }
}
