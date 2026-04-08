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
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponseDto<List<OrderDto>>>> GetUserOrders()
        {
            var currentUserId = GetCurrentUserId();
            var result = await _orderService.GetUserOrdersAsync(currentUserId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseDto<OrderDto>>> GetOrderById(int id)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _orderService.GetOrderByIdAsync(id, currentUserId);
            if (result.Success && result.Data != null)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseDto<OrderDto>>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _orderService.CreateOrderAsync(currentUserId, createOrderDto);
            if (result.Success)
            {
                return CreatedAtAction(nameof(GetOrderById), new { id = result.Data?.Id }, result);
            }
            return BadRequest(result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponseDto<OrderDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusDto);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<BaseResponseDto<string>>> CancelOrder(int id)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _orderService.CancelOrderAsync(id, currentUserId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BaseResponseDto<List<OrderDto>>>> GetAllOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _orderService.GetAllOrdersAsync(pageNumber, pageSize);
            return Ok(result);
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