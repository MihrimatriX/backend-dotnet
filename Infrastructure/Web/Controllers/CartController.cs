using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.Services;
using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ApiControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<CartDto>>> GetCart() =>
            Respond(await _cartService.GetCartAsync(CurrentUserId));

        [HttpPost("add")]
        public async Task<ActionResult<BaseResponseDto<CartDto>>> AddToCart([FromBody] AddToCartDto request) =>
            Respond(await _cartService.AddToCartAsync(CurrentUserId, request.ProductId, request.Quantity));

        [HttpPut("update")]
        public async Task<ActionResult<BaseResponseDto<CartDto>>> UpdateCartItem([FromBody] UpdateCartItemDto request) =>
            Respond(await _cartService.UpdateCartItemAsync(CurrentUserId, request.ProductId, request.Quantity));

        [HttpDelete("remove/{productId}")]
        public async Task<ActionResult<BaseResponseDto<bool>>> RemoveFromCart(int productId) =>
            Respond(await _cartService.RemoveFromCartAsync(CurrentUserId, productId));

        [HttpDelete("clear")]
        public async Task<ActionResult<BaseResponseDto<bool>>> ClearCart() =>
            Respond(await _cartService.ClearCartAsync(CurrentUserId));

        [HttpGet("total")]
        public async Task<ActionResult<BaseResponseDto<decimal>>> GetCartTotal() =>
            Respond(await _cartService.GetCartTotalAsync(CurrentUserId));

        [HttpGet("count")]
        public async Task<ActionResult<BaseResponseDto<int>>> GetCartItemCount() =>
            Respond(await _cartService.GetCartItemCountAsync(CurrentUserId));
    }
}
