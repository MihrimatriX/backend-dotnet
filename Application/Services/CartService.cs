using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EcommerceBackend.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartService> _logger;
        private readonly CheckoutOptions _checkoutOptions;

        public CartService(
            ApplicationDbContext context,
            ILogger<CartService> logger,
            IOptions<CheckoutOptions> checkoutOptions)
        {
            _context = context;
            _logger = logger;
            _checkoutOptions = checkoutOptions.Value;
        }

        private async Task<CartDto> BuildCartDtoAsync(int userId)
        {
            var rows = await _context.CartItems
                .AsNoTracking()
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && c.IsActive && c.Product.IsActive)
                .ToListAsync();

            var items = new List<CartItemDto>();
            foreach (var row in rows)
            {
                var p = row.Product;
                var unit = ProductPricing.EffectiveUnitPrice(p.UnitPrice, p.Discount);
                var requested = row.Quantity;
                var canFulfill = p.UnitInStock > 0;
                var fulfilledQty = canFulfill ? Math.Min(requested, p.UnitInStock) : 0;
                var isAvailable = canFulfill && fulfilledQty >= requested;

                items.Add(new CartItemDto
                {
                    ProductId = p.Id,
                    ProductName = p.ProductName,
                    UnitPrice = unit,
                    Quantity = requested,
                    TotalPrice = unit * fulfilledQty,
                    ProductImageUrl = p.ImageUrl,
                    IsAvailable = isAvailable,
                });
            }

            var subtotal = items.Sum(i => i.TotalPrice);
            var (_, shipping, grand) = ShippingQuote.Calculate(subtotal, _checkoutOptions);
            var remaining = ShippingQuote.FreeShippingRemaining(subtotal, _checkoutOptions);

            return new CartDto
            {
                UserId = userId,
                Items = items,
                TotalItems = items.Sum(i => i.Quantity),
                TotalAmount = subtotal,
                ShippingFee = shipping,
                GrandTotal = grand,
                FreeShippingRemainingTry = remaining,
            };
        }

        public async Task<BaseResponseDto<CartDto>> GetCartAsync(int userId)
        {
            try
            {
                var cart = await BuildCartDtoAsync(userId);
                return new BaseResponseDto<CartDto>
                {
                    Success = true,
                    Data = cart,
                    Message = "Sepet getirildi",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}", userId);
                return new BaseResponseDto<CartDto>
                {
                    Success = false,
                    Message = "Sepet yüklenemedi.",
                    ErrorCode = "CART_ERROR",
                };
            }
        }

        public async Task<BaseResponseDto<CartDto>> AddToCartAsync(int userId, int productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Adet 0'dan büyük olmalıdır.",
                        ErrorCode = "INVALID_QUANTITY",
                    };
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

                if (product == null)
                {
                    return new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Ürün bulunamadı veya satışta değil.",
                        ErrorCode = "PRODUCT_NOT_FOUND",
                    };
                }

                if (product.UnitInStock < 1)
                {
                    return new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Bu ürün stokta yok.",
                        ErrorCode = "OUT_OF_STOCK",
                    };
                }

                var line = await _context.CartItems.FirstOrDefaultAsync(c =>
                    c.UserId == userId && c.ProductId == productId && c.IsActive);

                var newQty = (line?.Quantity ?? 0) + quantity;
                newQty = Math.Min(newQty, product.UnitInStock);

                if (line == null)
                {
                    _context.CartItems.Add(new CartItem
                    {
                        UserId = userId,
                        ProductId = productId,
                        Quantity = newQty,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    });
                }
                else
                {
                    line.Quantity = newQty;
                    line.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                var cart = await BuildCartDtoAsync(userId);
                return new BaseResponseDto<CartDto>
                {
                    Success = true,
                    Data = cart,
                    Message = "Product added to cart successfully",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} to cart for user {UserId}", productId, userId);
                return new BaseResponseDto<CartDto>
                {
                    Success = false,
                    Message = "Error adding product to cart",
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
                    var afterRemove = await BuildCartDtoAsync(userId);
                    return new BaseResponseDto<CartDto>
                    {
                        Success = true,
                        Data = afterRemove,
                        Message = "Cart item removed",
                    };
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

                if (product == null)
                {
                    return new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Ürün bulunamadı veya satışta değil.",
                        ErrorCode = "PRODUCT_NOT_FOUND",
                    };
                }

                if (product.UnitInStock < quantity)
                {
                    return new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Stokta yeterli ürün yok. Miktarı düşürün.",
                        ErrorCode = "INSUFFICIENT_STOCK",
                    };
                }

                var line = await _context.CartItems.FirstOrDefaultAsync(c =>
                    c.UserId == userId && c.ProductId == productId && c.IsActive);

                if (line == null)
                {
                    return new BaseResponseDto<CartDto>
                    {
                        Success = false,
                        Message = "Bu ürün sepetinizde yok.",
                        ErrorCode = "NOT_IN_CART",
                    };
                }

                line.Quantity = quantity;
                line.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var cart = await BuildCartDtoAsync(userId);
                return new BaseResponseDto<CartDto>
                {
                    Success = true,
                    Data = cart,
                    Message = "Cart item updated successfully",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {ProductId} for user {UserId}", productId, userId);
                return new BaseResponseDto<CartDto>
                {
                    Success = false,
                    Message = "Error updating cart item",
                };
            }
        }

        public async Task<BaseResponseDto<bool>> RemoveFromCartAsync(int userId, int productId)
        {
            try
            {
                var line = await _context.CartItems.FirstOrDefaultAsync(c =>
                    c.UserId == userId && c.ProductId == productId && c.IsActive);

                if (line != null)
                {
                    _context.CartItems.Remove(line);
                    await _context.SaveChangesAsync();
                }

                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Product removed from cart successfully",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product {ProductId} from cart for user {UserId}", productId, userId);
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Error removing product from cart",
                };
            }
        }

        public async Task<BaseResponseDto<bool>> ClearCartAsync(int userId)
        {
            try
            {
                var lines = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();
                if (lines.Count > 0)
                {
                    _context.CartItems.RemoveRange(lines);
                    await _context.SaveChangesAsync();
                }

                return new BaseResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Cart cleared successfully",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                return new BaseResponseDto<bool>
                {
                    Success = false,
                    Message = "Error clearing cart",
                };
            }
        }

        public async Task<BaseResponseDto<decimal>> GetCartTotalAsync(int userId)
        {
            try
            {
                var cart = await BuildCartDtoAsync(userId);
                return new BaseResponseDto<decimal>
                {
                    Success = true,
                    Data = cart.GrandTotal,
                    Message = "Sepet toplamı (kargo dahil)",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart total for user {UserId}", userId);
                return new BaseResponseDto<decimal>
                {
                    Success = false,
                    Message = "Error getting cart total",
                };
            }
        }

        public async Task<BaseResponseDto<int>> GetCartItemCountAsync(int userId)
        {
            try
            {
                var cart = await BuildCartDtoAsync(userId);
                return new BaseResponseDto<int>
                {
                    Success = true,
                    Data = cart.TotalItems,
                    Message = "Cart item count retrieved successfully",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count for user {UserId}", userId);
                return new BaseResponseDto<int>
                {
                    Success = false,
                    Message = "Error getting cart item count",
                };
            }
        }
    }
}
