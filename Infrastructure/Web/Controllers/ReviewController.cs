using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponseDto<List<ReviewDto>>>> GetReviewsAdmin(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await _reviewService.GetReviewsAdminAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<BaseResponseDto<IEnumerable<ReviewDto>>>> GetProductReviews(int productId)
        {
            var result = await _reviewService.GetProductReviewsAsync(productId);
            return Ok(result);
        }

        [HttpGet("product/{productId}/summary")]
        public async Task<ActionResult<BaseResponseDto<ProductReviewSummaryDto>>> GetProductReviewSummary(int productId)
        {
            var result = await _reviewService.GetProductReviewSummaryAsync(productId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseDto<ReviewDto>>> GetReview(int id)
        {
            var result = await _reviewService.GetReviewByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BaseResponseDto<ReviewDto>>> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(BaseResponseDto<ReviewDto>.ErrorResult("Invalid user"));

            var result = await _reviewService.CreateReviewAsync(createReviewDto, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetReview), new { id = result.Data!.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<BaseResponseDto<ReviewDto>>> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(BaseResponseDto<ReviewDto>.ErrorResult("Invalid user"));

            var isAdmin = User.IsInRole("Admin");
            var result = await _reviewService.UpdateReviewAsync(id, updateReviewDto, userId, isAdmin);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteReview(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(BaseResponseDto<string>.ErrorResult("Invalid user"));

            var isAdmin = User.IsInRole("Admin");
            var result = await _reviewService.DeleteReviewAsync(id, userId, isAdmin);

            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}
