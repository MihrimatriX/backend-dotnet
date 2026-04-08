using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IOrderService
    {
        Task<BaseResponseDto<List<OrderDto>>> GetUserOrdersAsync(int userId);
        Task<BaseResponseDto<OrderDto>> GetOrderByIdAsync(int orderId, int userId);
        Task<BaseResponseDto<OrderDto>> CreateOrderAsync(int userId, CreateOrderDto createOrderDto);
        Task<BaseResponseDto<OrderDto>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto);
        Task<BaseResponseDto<string>> CancelOrderAsync(int orderId, int userId);
        Task<BaseResponseDto<List<OrderDto>>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 10);
    }
}