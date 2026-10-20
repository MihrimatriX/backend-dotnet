using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    public class ReviewController : ApiControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<List<ReviewDto>>>> GetReviewsAdmin(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50) =>
            Respond(await _reviewService.GetReviewsAdminAsync(pageNumber, pageSize));

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<BaseResponseDto<IEnumerable<ReviewDto>>>> GetProductReviews(int productId) =>
            Respond(await _reviewService.GetProductReviewsAsync(productId));

        [HttpGet("product/{productId}/summary")]
        public async Task<ActionResult<BaseResponseDto<ProductReviewSummaryDto>>> GetProductReviewSummary(int productId) =>
            Respond(await _reviewService.GetProductReviewSummaryAsync(productId));

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseDto<ReviewDto>>> GetReview(int id) =>
            Respond(await _reviewService.GetReviewByIdAsync(id));

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BaseResponseDto<ReviewDto>>> CreateReview([FromBody] CreateReviewDto createReviewDto) =>
            RespondCreated(await _reviewService.CreateReviewAsync(createReviewDto, CurrentUserId));

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<BaseResponseDto<ReviewDto>>> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto) =>
            Respond(await _reviewService.UpdateReviewAsync(id, updateReviewDto, CurrentUserId, IsAdmin));

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<BaseResponseDto<string>>> DeleteReview(int id) =>
            Respond(await _reviewService.DeleteReviewAsync(id, CurrentUserId, IsAdmin));
    }
}
