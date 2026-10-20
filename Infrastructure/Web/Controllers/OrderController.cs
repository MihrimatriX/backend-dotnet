using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Services;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ApiControllerBase
    {
        public const string IdempotencyKeyHeader = "Idempotency-Key";

        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<List<OrderDto>>>> GetUserOrders() =>
            Respond(await _orderService.GetUserOrdersAsync(CurrentUserId));

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseDto<OrderDto>>> GetOrderById(int id) =>
            Respond(await _orderService.GetOrderByIdAsync(id, CurrentUserId));

        /// <summary>201 + sipariş; aynı <c>Idempotency-Key</c> tekrarında 200 + ilk sipariş.</summary>
        [HttpPost]
        public async Task<ActionResult<BaseResponseDto<OrderDto>>> CreateOrder(
            [FromBody] CreateOrderDto createOrderDto,
            [FromHeader(Name = IdempotencyKeyHeader)] string? idempotencyKey) =>
            RespondCreated(await _orderService.CreateOrderAsync(CurrentUserId, createOrderDto, idempotencyKey));

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<BaseResponseDto<string>>> CancelOrder(
            int id,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CancelOrderDto? request) =>
            Respond(await _orderService.CancelOrderAsync(id, CurrentUserId, request?.Reason));

        [HttpPost("{id}/return-request")]
        public async Task<ActionResult<BaseResponseDto<OrderDto>>> RequestReturn(int id, [FromBody] ReturnRequestDto request) =>
            Respond(await _orderService.RequestReturnAsync(id, CurrentUserId, request.Reason));

        [HttpPost("{id}/demo/advance-fulfillment")]
        public async Task<ActionResult<BaseResponseDto<OrderDto>>> AdvanceDemoFulfillment(int id) =>
            Respond(await _orderService.AdvanceDemoFulfillmentAsync(id, CurrentUserId));

        [HttpPut("{id}/status")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<OrderDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateOrderStatusDto) =>
            Respond(await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusDto));

        [HttpGet("admin")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<BaseResponseDto<List<OrderDto>>>> GetAllOrders(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20) =>
            Respond(await _orderService.GetAllOrdersAsync(pageNumber, pageSize));
    }
}
