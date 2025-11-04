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
    public class PaymentMethodController : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;

        public PaymentMethodController(IPaymentMethodService paymentMethodService)
        {
            _paymentMethodService = paymentMethodService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<BaseResponseDto<List<PaymentMethodDto>>>> GetUserPaymentMethods(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId != userId)
                {
                    return Forbid("You can only access your own payment methods");
                }

                var result = await _paymentMethodService.GetUserPaymentMethodsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<List<PaymentMethodDto>>.ErrorResult($"Error retrieving user payment methods: {ex.Message}"));
            }
        }

        [HttpGet("{paymentMethodId}")]
        public async Task<ActionResult<BaseResponseDto<PaymentMethodDto>>> GetPaymentMethod(int paymentMethodId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _paymentMethodService.GetPaymentMethodByIdAsync(paymentMethodId, currentUserId);
                
                if (result.Success && result.Data != null)
                {
                    return Ok(result);
                }
                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<PaymentMethodDto>.ErrorResult($"Error retrieving payment method: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseDto<PaymentMethodDto>>> CreatePaymentMethod([FromBody] CreatePaymentMethodDto createPaymentMethodDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _paymentMethodService.CreatePaymentMethodAsync(currentUserId, createPaymentMethodDto);
                
                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetPaymentMethod), new { paymentMethodId = result.Data?.Id }, result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<PaymentMethodDto>.ErrorResult($"Error creating payment method: {ex.Message}"));
            }
        }

        [HttpPut("{paymentMethodId}")]
        public async Task<ActionResult<BaseResponseDto<PaymentMethodDto>>> UpdatePaymentMethod(int paymentMethodId, [FromBody] UpdatePaymentMethodDto updatePaymentMethodDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _paymentMethodService.UpdatePaymentMethodAsync(paymentMethodId, currentUserId, updatePaymentMethodDto);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<PaymentMethodDto>.ErrorResult($"Error updating payment method: {ex.Message}"));
            }
        }

        [HttpDelete("{paymentMethodId}")]
        public async Task<ActionResult<BaseResponseDto<string>>> DeletePaymentMethod(int paymentMethodId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _paymentMethodService.DeletePaymentMethodAsync(paymentMethodId, currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<string>.ErrorResult($"Error deleting payment method: {ex.Message}"));
            }
        }

        [HttpPut("{paymentMethodId}/default")]
        public async Task<ActionResult<BaseResponseDto<PaymentMethodDto>>> SetDefaultPaymentMethod(int paymentMethodId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _paymentMethodService.SetDefaultPaymentMethodAsync(paymentMethodId, currentUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, BaseResponseDto<PaymentMethodDto>.ErrorResult($"Error setting default payment method: {ex.Message}"));
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
