using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface ICartService
    {
        Task<BaseResponseDto<CartDto>> GetCartAsync(int userId);
        Task<BaseResponseDto<CartDto>> AddToCartAsync(int userId, int productId, int quantity);
        Task<BaseResponseDto<CartDto>> UpdateCartItemAsync(int userId, int productId, int quantity);
        Task<BaseResponseDto<bool>> RemoveFromCartAsync(int userId, int productId);
        Task<BaseResponseDto<bool>> ClearCartAsync(int userId);
        Task<BaseResponseDto<decimal>> GetCartTotalAsync(int userId);
        Task<BaseResponseDto<int>> GetCartItemCountAsync(int userId);
    }
}
