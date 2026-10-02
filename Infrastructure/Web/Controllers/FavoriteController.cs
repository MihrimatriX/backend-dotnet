using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class FavoriteController : ApiControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<List<FavoriteDto>>>> GetUserFavorites() =>
            Respond(await _favoriteService.GetUserFavoritesAsync(CurrentUserId));

        [HttpPost("add")]
        public async Task<ActionResult<BaseResponseDto<FavoriteDto>>> AddToFavorites([FromBody] AddToFavoritesDto addToFavoritesDto) =>
            Respond(await _favoriteService.AddToFavoritesAsync(CurrentUserId, addToFavoritesDto));

        [HttpDelete("remove/{productId}")]
        public async Task<ActionResult<BaseResponseDto<string>>> RemoveFromFavorites(int productId) =>
            Respond(await _favoriteService.RemoveFromFavoritesAsync(CurrentUserId, productId));

        [HttpGet("check/{productId}")]
        public async Task<ActionResult<BaseResponseDto<bool>>> IsProductInFavorites(int productId) =>
            Respond(await _favoriteService.IsProductInFavoritesAsync(CurrentUserId, productId));

        [HttpDelete("clear")]
        public async Task<ActionResult<BaseResponseDto<string>>> ClearFavorites() =>
            Respond(await _favoriteService.ClearFavoritesAsync(CurrentUserId));
    }
}
