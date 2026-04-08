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
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<List<FavoriteDto>>>> GetUserFavorites()
        {
            var currentUserId = GetCurrentUserId();
            var result = await _favoriteService.GetUserFavoritesAsync(currentUserId);
            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<ActionResult<BaseResponseDto<FavoriteDto>>> AddToFavorites([FromBody] AddToFavoritesDto addToFavoritesDto)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _favoriteService.AddToFavoritesAsync(currentUserId, addToFavoritesDto);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("remove/{productId}")]
        public async Task<ActionResult<BaseResponseDto<string>>> RemoveFromFavorites(int productId)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _favoriteService.RemoveFromFavoritesAsync(currentUserId, productId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("check/{productId}")]
        public async Task<ActionResult<BaseResponseDto<bool>>> IsProductInFavorites(int productId)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _favoriteService.IsProductInFavoritesAsync(currentUserId, productId);
            return Ok(result);
        }

        [HttpDelete("clear")]
        public async Task<ActionResult<BaseResponseDto<string>>> ClearFavorites()
        {
            var currentUserId = GetCurrentUserId();
            var result = await _favoriteService.ClearFavoritesAsync(currentUserId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
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