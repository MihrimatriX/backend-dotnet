using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IOrderService
    {
        Task<BaseResponseDto<List<OrderDto>>> GetUserOrdersAsync(int userId);
        Task<BaseResponseDto<OrderDto>> GetOrderByIdAsync(int orderId, int userId);

        /// <param name="idempotencyKey"><c>Idempotency-Key</c> başlığının ham değeri (yoksa null).</param>
        Task<BaseResponseDto<OrderDto>> CreateOrderAsync(int userId, CreateOrderDto createOrderDto, string? idempotencyKey);

        Task<BaseResponseDto<OrderDto>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto);
        Task<BaseResponseDto<string>> CancelOrderAsync(int orderId, int userId, string? reason);
        Task<BaseResponseDto<OrderDto>> RequestReturnAsync(int orderId, int userId, string reason);
        Task<BaseResponseDto<OrderDto>> AdvanceDemoFulfillmentAsync(int orderId, int userId);
        Task<BaseResponseDto<List<OrderDto>>> GetAllOrdersAsync(int pageNumber, int pageSize);
    }
}
