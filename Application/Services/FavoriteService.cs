using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Repositories;

namespace EcommerceBackend.Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IProductRepository _productRepository;

        public FavoriteService(IFavoriteRepository favoriteRepository, IProductRepository productRepository)
        {
            _favoriteRepository = favoriteRepository;
            _productRepository = productRepository;
        }

        public async Task<BaseResponseDto<List<FavoriteDto>>> GetUserFavoritesAsync(int userId)
        {
            try
            {
                var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId);
                var favoriteDtos = favorites.Select(ConvertToDto).ToList();

                return BaseResponseDto<List<FavoriteDto>>.SuccessResult("Favorites retrieved successfully", favoriteDtos);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<List<FavoriteDto>>.ErrorResult($"Error retrieving favorites: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<FavoriteDto>> AddToFavoritesAsync(int userId, AddToFavoritesDto addToFavoritesDto)
        {
            try
            {
                // Check if product exists and is active
                var product = await _productRepository.GetByIdAsync(addToFavoritesDto.ProductId);
                if (product == null || !product.IsActive)
                {
                    return BaseResponseDto<FavoriteDto>.ErrorResult("Product not found or inactive");
                }

                // Check if already in favorites
                var existingFavorite = await _favoriteRepository.GetUserFavoriteAsync(userId, addToFavoritesDto.ProductId);
                if (existingFavorite != null)
                {
                    return BaseResponseDto<FavoriteDto>.ErrorResult("Product already in favorites");
                }

                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = addToFavoritesDto.ProductId,
                    CreatedAt = DateTime.UtcNow
                };

                var createdFavorite = await _favoriteRepository.CreateAsync(favorite);
                return BaseResponseDto<FavoriteDto>.SuccessResult("Product added to favorites", ConvertToDto(createdFavorite));
            }
            catch (Exception ex)
            {
                return BaseResponseDto<FavoriteDto>.ErrorResult($"Error adding to favorites: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> RemoveFromFavoritesAsync(int userId, int productId)
        {
            try
            {
                var favorite = await _favoriteRepository.GetUserFavoriteAsync(userId, productId);
                if (favorite == null)
                {
                    return BaseResponseDto<string>.ErrorResult("Product not found in favorites");
                }

                await _favoriteRepository.DeleteAsync(userId, productId);
                return BaseResponseDto<string>.SuccessResult("Product removed from favorites", "Product removed from favorites");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error removing from favorites: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<bool>> IsProductInFavoritesAsync(int userId, int productId)
        {
            try
            {
                var favorite = await _favoriteRepository.GetUserFavoriteAsync(userId, productId);
                var isInFavorites = favorite != null;

                return BaseResponseDto<bool>.SuccessResult("Favorite status retrieved", isInFavorites);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<bool>.ErrorResult($"Error checking favorite status: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> ClearFavoritesAsync(int userId)
        {
            try
            {
                await _favoriteRepository.ClearUserFavoritesAsync(userId);
                return BaseResponseDto<string>.SuccessResult("Favorites cleared successfully", "Favorites cleared successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error clearing favorites: {ex.Message}");
            }
        }

        private FavoriteDto ConvertToDto(Favorite favorite)
        {
            return new FavoriteDto
            {
                Id = favorite.Id,
                UserId = favorite.UserId,
                ProductId = favorite.ProductId,
                ProductName = favorite.Product?.ProductName ?? "",
                ProductImageUrl = favorite.Product?.ImageUrl,
                ProductPrice = favorite.Product?.UnitPrice ?? 0,
                ProductDiscount = favorite.Product?.Discount,
                ProductCategory = favorite.Product?.Category?.CategoryName,
                ProductInStock = favorite.Product?.UnitInStock > 0,
                CreatedAt = favorite.CreatedAt
            };
        }
    }
}