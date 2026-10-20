using EcommerceBackend.Application.Common;
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
            var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId);
            return BaseResponseDto<List<FavoriteDto>>.SuccessResult(
                "Favorites retrieved successfully",
                favorites.Select(f => ToDto(f, f.Product)).ToList());
        }

        public async Task<BaseResponseDto<FavoriteDto>> AddToFavoritesAsync(int userId, AddToFavoritesDto addToFavoritesDto)
        {
            var product = await _productRepository.GetActiveByIdAsync(addToFavoritesDto.ProductId);
            if (product == null)
                return BaseResponseDto<FavoriteDto>.Fail("Product not found or inactive", ErrorCodes.ProductNotFound);

            // (kullanıcı, ürün) benzersiz indeksli; daha önce kaldırılmış satır yeniden etkinleştirilir.
            var favorite = await _favoriteRepository.GetUserFavoriteAsync(userId, product.Id, includeInactive: true);
            if (favorite is { IsActive: true })
                return BaseResponseDto<FavoriteDto>.Fail("Product already in favorites", ErrorCodes.AlreadyFavorite);

            if (favorite == null)
            {
                favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = product.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                await _favoriteRepository.AddAsync(favorite);
            }
            else
            {
                favorite.IsActive = true;
                favorite.CreatedAt = DateTime.UtcNow;
                favorite.UpdatedAt = DateTime.UtcNow;
                await _favoriteRepository.SaveChangesAsync();
            }

            return BaseResponseDto<FavoriteDto>.SuccessResult("Product added to favorites", ToDto(favorite, product));
        }

        public async Task<BaseResponseDto<string>> RemoveFromFavoritesAsync(int userId, int productId)
        {
            var favorite = await _favoriteRepository.GetUserFavoriteAsync(userId, productId);
            if (favorite == null)
                return BaseResponseDto<string>.Fail("Product not found in favorites", ErrorCodes.NotFavorite);

            favorite.IsActive = false;
            favorite.UpdatedAt = DateTime.UtcNow;
            await _favoriteRepository.SaveChangesAsync();

            return BaseResponseDto<string>.SuccessResult("Product removed from favorites", "Product removed from favorites");
        }

        public async Task<BaseResponseDto<bool>> IsProductInFavoritesAsync(int userId, int productId)
        {
            var favorite = await _favoriteRepository.GetUserFavoriteAsync(userId, productId);
            return BaseResponseDto<bool>.SuccessResult("Favorite status retrieved", favorite != null);
        }

        public async Task<BaseResponseDto<string>> ClearFavoritesAsync(int userId)
        {
            await _favoriteRepository.ClearUserFavoritesAsync(userId);
            return BaseResponseDto<string>.SuccessResult("Favorites cleared successfully", "Favorites cleared successfully");
        }

        private static FavoriteDto ToDto(Favorite favorite, Product? product) => new()
        {
            Id = favorite.Id,
            UserId = favorite.UserId,
            ProductId = favorite.ProductId,
            ProductName = product?.ProductName ?? string.Empty,
            ProductImageUrl = product?.ImageUrl,
            ProductPrice = product?.UnitPrice ?? 0,
            ProductDiscount = product?.Discount,
            ProductCategory = product?.Category?.CategoryName,
            ProductInStock = product?.UnitInStock > 0,
            CreatedAt = favorite.CreatedAt
        };
    }
}
