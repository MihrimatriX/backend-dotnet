using EcommerceBackend.Application.Common;
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
        private readonly CheckoutOptions _checkoutOptions;

        public CartService(ApplicationDbContext context, IOptions<CheckoutOptions> checkoutOptions)
        {
            _context = context;
            _checkoutOptions = checkoutOptions.Value;
        }

        /// <summary>
        /// Sepet (§5.1): satır fiyatı = indirimli satış fiyatı; satır tutarı stokla sınırlı miktar üzerinden;
        /// pasif ürünler gösterilmez.
        /// </summary>
        private async Task<CartDto> BuildCartDtoAsync(int userId)
        {
            var rows = await _context.CartItems
                .AsNoTracking()
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && c.IsActive && c.Product.IsActive)
                .OrderBy(c => c.CreatedAt)
                .ThenBy(c => c.Id)
                .ToListAsync();

            var items = rows.Select(row =>
            {
                var product = row.Product;
                var unit = ProductPricing.EffectiveUnitPrice(product.UnitPrice, product.Discount);
                var fulfilledQty = Math.Min(row.Quantity, Math.Max(0, product.UnitInStock));
                return new CartItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    ProductImageUrl = product.ImageUrl,
                    UnitPrice = unit,
                    Quantity = row.Quantity,
                    TotalPrice = unit * fulfilledQty,
                    IsAvailable = product.UnitInStock > 0 && product.UnitInStock >= row.Quantity,
                };
            }).ToList();

            var (subtotal, shipping, grand) = ShippingQuote.Calculate(items.Sum(i => i.TotalPrice), _checkoutOptions);

            return new CartDto
            {
                UserId = userId,
                Items = items,
                TotalItems = items.Sum(i => i.Quantity),
                TotalAmount = subtotal,
                ShippingFee = shipping,
                GrandTotal = grand,
                FreeShippingRemainingTry = ShippingQuote.FreeShippingRemaining(subtotal, _checkoutOptions),
            };
        }

        public async Task<BaseResponseDto<CartDto>> GetCartAsync(int userId) =>
            BaseResponseDto<CartDto>.SuccessResult("Sepet getirildi", await BuildCartDtoAsync(userId));

        /// <summary>Miktar mevcut satıra eklenir ve stokla sınırlanır.</summary>
        public async Task<BaseResponseDto<CartDto>> AddToCartAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0)
                return BaseResponseDto<CartDto>.Fail("Adet 0'dan büyük olmalıdır.", ErrorCodes.InvalidQuantity);

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
            if (product == null)
                return ProductUnavailable();

            if (product.UnitInStock < 1)
                return BaseResponseDto<CartDto>.Fail("Bu ürün stokta yok.", ErrorCodes.OutOfStock);

            var line = await FindLineAsync(userId, productId);
            var newQty = Math.Min((line?.Quantity ?? 0) + quantity, product.UnitInStock);

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
            return BaseResponseDto<CartDto>.SuccessResult("Product added to cart successfully", await BuildCartDtoAsync(userId));
        }

        /// <summary>Miktarı ayarlar; <c>quantity ≤ 0</c> satırı siler.</summary>
        public async Task<BaseResponseDto<CartDto>> UpdateCartItemAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0)
            {
                await RemoveLineAsync(userId, productId);
                return BaseResponseDto<CartDto>.SuccessResult("Cart item removed", await BuildCartDtoAsync(userId));
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
            if (product == null)
                return ProductUnavailable();

            if (product.UnitInStock < quantity)
                return BaseResponseDto<CartDto>.Fail("Stokta yeterli ürün yok. Miktarı düşürün.", ErrorCodes.InsufficientStock);

            var line = await FindLineAsync(userId, productId);
            if (line == null)
                return BaseResponseDto<CartDto>.Fail("Bu ürün sepetinizde yok.", ErrorCodes.NotInCart);

            line.Quantity = quantity;
            line.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<CartDto>.SuccessResult("Cart item updated successfully", await BuildCartDtoAsync(userId));
        }

        /// <summary>İdempotent: satır yoksa da başarılıdır.</summary>
        public async Task<BaseResponseDto<bool>> RemoveFromCartAsync(int userId, int productId)
        {
            await RemoveLineAsync(userId, productId);
            return BaseResponseDto<bool>.SuccessResult("Product removed from cart successfully", true);
        }

        public async Task<BaseResponseDto<bool>> ClearCartAsync(int userId)
        {
            var lines = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            if (lines.Count > 0)
            {
                _context.CartItems.RemoveRange(lines);
                await _context.SaveChangesAsync();
            }

            return BaseResponseDto<bool>.SuccessResult("Cart cleared successfully", true);
        }

        public async Task<BaseResponseDto<decimal>> GetCartTotalAsync(int userId) =>
            BaseResponseDto<decimal>.SuccessResult("Sepet toplamı (kargo dahil)", (await BuildCartDtoAsync(userId)).GrandTotal);

        public async Task<BaseResponseDto<int>> GetCartItemCountAsync(int userId) =>
            BaseResponseDto<int>.SuccessResult("Cart item count retrieved successfully", (await BuildCartDtoAsync(userId)).TotalItems);

        private Task<CartItem?> FindLineAsync(int userId, int productId) =>
            _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId && c.IsActive);

        private async Task RemoveLineAsync(int userId, int productId)
        {
            var line = await FindLineAsync(userId, productId);
            if (line == null)
                return;

            _context.CartItems.Remove(line);
            await _context.SaveChangesAsync();
        }

        private static BaseResponseDto<CartDto> ProductUnavailable() =>
            BaseResponseDto<CartDto>.Fail("Ürün bulunamadı veya satışta değil.", ErrorCodes.ProductNotFound);
    }
}
