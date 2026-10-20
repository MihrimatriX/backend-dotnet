using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentMethodController : ApiControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;

        public PaymentMethodController(IPaymentMethodService paymentMethodService)
        {
            _paymentMethodService = paymentMethodService;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<BaseResponseDto<List<PaymentMethodDto>>>> GetUserPaymentMethods(int userId)
        {
            if (CurrentUserId != userId)
                return Respond(BaseResponseDto<List<PaymentMethodDto>>.Forbidden("You can only access your own payment methods"));

            return Respond(await _paymentMethodService.GetUserPaymentMethodsAsync(userId));
        }

        [HttpGet("{paymentMethodId}")]
        public async Task<ActionResult<BaseResponseDto<PaymentMethodDto>>> GetPaymentMethod(int paymentMethodId) =>
            Respond(await _paymentMethodService.GetPaymentMethodByIdAsync(paymentMethodId, CurrentUserId));

        [HttpPost]
        public async Task<ActionResult<BaseResponseDto<PaymentMethodDto>>> CreatePaymentMethod([FromBody] CreatePaymentMethodDto createPaymentMethodDto) =>
            RespondCreated(await _paymentMethodService.CreatePaymentMethodAsync(CurrentUserId, createPaymentMethodDto));

        [HttpPut("{paymentMethodId}")]
        public async Task<ActionResult<BaseResponseDto<PaymentMethodDto>>> UpdatePaymentMethod(int paymentMethodId, [FromBody] UpdatePaymentMethodDto updatePaymentMethodDto) =>
            Respond(await _paymentMethodService.UpdatePaymentMethodAsync(paymentMethodId, CurrentUserId, updatePaymentMethodDto));

        [HttpDelete("{paymentMethodId}")]
        public async Task<ActionResult<BaseResponseDto<string>>> DeletePaymentMethod(int paymentMethodId) =>
            Respond(await _paymentMethodService.DeletePaymentMethodAsync(paymentMethodId, CurrentUserId));

        [HttpPut("{paymentMethodId}/default")]
        public async Task<ActionResult<BaseResponseDto<PaymentMethodDto>>> SetDefaultPaymentMethod(int paymentMethodId) =>
            Respond(await _paymentMethodService.SetDefaultPaymentMethodAsync(paymentMethodId, CurrentUserId));
    }
}
