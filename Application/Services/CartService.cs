using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(ApplicationDbContext context, ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BaseResponseDto<CartDto>> GetCartAsync(int userId)
        {
            try
            {
                var cartItems = await _context.Products
                    .Where(p => p.IsActive)
                    .Select(p => new CartItemDto
                    {
                        ProductId = p.Id,
                        ProductName = p.ProductName,
                        UnitPrice = p.UnitPrice,
                        Quantity = 0, // This would come from a separate cart table in a real implementation
                        TotalPrice = 0,
                        ProductImageUrl = p.ImageUrl,
                        IsAvailable = p.UnitInStock > 0
                    })
                    .ToListAsync();

                var cart = new CartDto
                {
                    UserId = userId,
                    Items = cartItems,
                    TotalItems = cartItems.Sum(i => i.Quantity),
                    TotalAmount = cartItems.Sum(i => i.TotalPrice)
                };

                return new BaseResponseDto<CartDto>
                {
                    Success = true,
                    Data = cart,
                    Message = "Cart retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}", userId);
                return new BaseResponseDto<CartDto>
                {
                    Success = false,
                    Message = "Error retrieving cart"
                };
            }
        }

        public async Task<BaseResponseDto<CartDto>> AddToCartAsync(int userId, int productId, int quantity)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

                if (product == null)
                {
                    return new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    };
                }

                if (product.UnitInStock < quantity)
                {
                    return new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Insufficient stock"
                    };
                }

                // In a real implementation, you would have a Cart table
                // For now, we'll simulate the cart operations
                var cart = await GetCartAsync(userId);
                
                return new BaseResponseDto<CartDto>
                {
                    Success = true,
                    Data = cart.Data,
                    Message = "Product added to cart successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart for user {UserId}", productId, userId);
                return new BaseResponseDto<CartDto>
                {
                    Success = false,
                    Message = "Error adding product to cart"
                };
            }
        }

        public async Task<BaseResponseDto<CartDto>> UpdateCartItemAsync(int userId, int productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    await RemoveFromCartAsync(userId, productId);
                    var updatedCart = await GetCartAsync(userId);
                    return updatedCart;
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

                if (product == null)
                {
                    return new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Product not found"
                    };
                }

                if (product.UnitInStock < quantity)
                {
                    return new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Insufficient stock"
                    };
                }

                // In a real implementation, you would update the cart table
                var cart = await GetCartAsync(userId);
                
                return new BaseResponseDto<CartDto>
                {
                    Success = true,
                    Data = cart.Data,
                    Message = "Cart item updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {ProductId} for user {UserId}", productId, userId);
                return new BaseResponseDto<CartDto>
                {
                    Success = false,
                    Message = "Error updating cart item"
                };
            }
        }

        public async Task<BaseResponseDto<bool>> RemoveFromCartAsync(int userId, int productId)
        {
            try
            {
                // In a real implementation, you would remove from cart table
                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product removed from cart successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product {ProductId} from cart for user {UserId}", productId, userId);
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Error removing product from cart"
                };
            }
        }

        public async Task<BaseResponseDto<bool>> ClearCartAsync(int userId)
        {
            try
            {
                // In a real implementation, you would clear the cart table
                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Cart cleared successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Error clearing cart"
                };
            }
        }

        public async Task<BaseResponseDto<decimal>> GetCartTotalAsync(int userId)
        {
            try
            {
                var cart = await GetCartAsync(userId);
                return new BaseResponseDto<decimal>
                {
                    Success = true,
                    Data = cart.Data?.TotalAmount ?? 0,
                    Message = "Cart total retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart total for user {UserId}", userId);
                return new BaseResponseDto<decimal>
                {
                    Success = false,
                    Message = "Error getting cart total"
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetCartItemCountAsync(int userId)
        {
            try
            {
                var cart = await GetCartAsync(userId);
                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = cart.Data?.TotalItems ?? 0,
                    Message = "Cart item count retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count for user {UserId}", userId);
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error getting cart item count"
                };
            }
        }
    }
}
