using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IFavoriteService
    {
        Task<BaseResponseDto<List<FavoriteDto>>> GetUserFavoritesAsync(int userId);
        Task<BaseResponseDto<FavoriteDto>> AddToFavoritesAsync(int userId, AddToFavoritesDto addToFavoritesDto);
        Task<BaseResponseDto<string>> RemoveFromFavoritesAsync(int userId, int productId);
        Task<BaseResponseDto<bool>> IsProductInFavoritesAsync(int userId, int productId);
        Task<BaseResponseDto<string>> ClearFavoritesAsync(int userId);
    }
}