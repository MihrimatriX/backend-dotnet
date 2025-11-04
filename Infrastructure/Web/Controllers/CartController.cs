using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.Services;
using EcommerceBackend.Application.DTOs;
using System.Security.Claims;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<CartDto>>> GetCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var result = await _cartService.GetCartAsync(userId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, new BaseResponseDto<CartDto>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPost("add")]
        public async Task<ActionResult<BaseResponseDto<CartDto>>> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (request.Quantity <= 0)
                {
                    return BadRequest(new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Quantity must be greater than 0"
                    });
                }

                var result = await _cartService.AddToCartAsync(userId.Value, request.ProductId, request.Quantity);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return StatusCode(500, new BaseResponseDto<CartDto>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpPut("update")]
        public async Task<ActionResult<BaseResponseDto<CartDto>>> UpdateCartItem([FromBody] UpdateCartItemRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var result = await _cartService.UpdateCartItemAsync(userId.Value, request.ProductId, request.Quantity);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return StatusCode(500, new BaseResponseDto<CartDto>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpDelete("remove/{productId}")]
        public async Task<ActionResult<BaseResponseDto<bool>>> RemoveFromCart(int productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new BaseResponseDto<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var result = await _cartService.RemoveFromCartAsync(userId.Value, productId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart");
                return StatusCode(500, new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpDelete("clear")]
        public async Task<ActionResult<BaseResponseDto<bool>>> ClearCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new BaseResponseDto<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var result = await _cartService.ClearCartAsync(userId.Value);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("total")]
        public async Task<ActionResult<BaseResponseDto<decimal>>> GetCartTotal()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new BaseResponseDto<decimal>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var result = await _cartService.GetCartTotalAsync(userId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart total");
                return StatusCode(500, new BaseResponseDto<decimal>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult<BaseResponseDto<int>>> GetCartItemCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new BaseResponseDto<int>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var result = await _cartService.GetCartItemCountAsync(userId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count");
                return StatusCode(500, new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}