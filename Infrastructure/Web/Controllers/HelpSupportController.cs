using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;
using System.Security.Claims;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelpSupportController : ControllerBase
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
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _helpSupportService.GetHelpArticlesAsync(category, pageNumber, pageSize);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<List<HelpArticleDto>>.ErrorResult($"Error retrieving help articles: {ex.Message}"));
            }
        }

        [HttpGet("articles/{articleId}")]
        public async Task<ActionResult<BaseResponseDto<HelpArticleDto>>> GetHelpArticle(int articleId)
        {
            try
            {
                var result = await _helpSupportService.GetHelpArticleByIdAsync(articleId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<HelpArticleDto>.ErrorResult($"Error retrieving help article: {ex.Message}"));
            }
        }

        [HttpPost("articles")]
        [Authorize]
        public async Task<ActionResult<BaseResponseDto<HelpArticleDto>>> CreateHelpArticle([FromBody] CreateHelpArticleDto createHelpArticleDto)
        {
            try
            {
                var result = await _helpSupportService.CreateHelpArticleAsync(createHelpArticleDto);
                
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetHelpArticle), new { articleId = result.Data?.Id }, result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<HelpArticleDto>.ErrorResult($"Error creating help article: {ex.Message}"));
            }
        }

        [HttpGet("faqs")]
        public async Task<ActionResult<BaseResponseDto<List<FaqDto>>>> GetFaqs(
            [FromQuery] string? category = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _helpSupportService.GetFaqsAsync(category, pageNumber, pageSize);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<List<FaqDto>>.ErrorResult($"Error retrieving FAQs: {ex.Message}"));
            }
        }

        [HttpPost("contact")]
        public async Task<ActionResult<BaseResponseDto<string>>> SubmitContactForm([FromBody] ContactFormDto contactFormDto)
        {
            try
            {
                var result = await _helpSupportService.SubmitContactFormAsync(contactFormDto);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error submitting contact form: {ex.Message}"));
            }
        }

        [HttpGet("tickets")]
        [Authorize]
        public async Task<ActionResult<BaseResponseDto<List<SupportTicketDto>>>> GetUserSupportTickets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _helpSupportService.GetUserSupportTicketsAsync(currentUserId, pageNumber, pageSize);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<List<SupportTicketDto>>.ErrorResult($"Error retrieving support tickets: {ex.Message}"));
            }
        }

        [HttpPost("tickets")]
        [Authorize]
        public async Task<ActionResult<BaseResponseDto<SupportTicketDto>>> CreateSupportTicket([FromBody] CreateSupportTicketDto createSupportTicketDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _helpSupportService.CreateSupportTicketAsync(currentUserId, createSupportTicketDto);
                
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetUserSupportTickets), new { }, result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<SupportTicketDto>.ErrorResult($"Error creating support ticket: {ex.Message}"));
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
