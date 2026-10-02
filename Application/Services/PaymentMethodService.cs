using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponseDto<List<PaymentMethodDto>>> GetUserPaymentMethodsAsync(int userId)
        {
            var paymentMethods = await _context.PaymentMethods
                .AsNoTracking()
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .OrderByDescending(pm => pm.IsDefault)
                .ThenByDescending(pm => pm.CreatedAt)
                .ThenByDescending(pm => pm.Id)
                .ToListAsync();

            return BaseResponseDto<List<PaymentMethodDto>>.SuccessResult(
                "Payment methods retrieved successfully",
                paymentMethods.Select(ToDto).ToList());
        }

        public async Task<BaseResponseDto<PaymentMethodDto>> GetPaymentMethodByIdAsync(int paymentMethodId, int userId)
        {
            var paymentMethod = await FindOwnedAsync(paymentMethodId, userId);
            return paymentMethod == null
                ? PaymentMethodNotFound<PaymentMethodDto>()
                : BaseResponseDto<PaymentMethodDto>.SuccessResult("Payment method retrieved successfully", ToDto(paymentMethod));
        }

        public async Task<BaseResponseDto<PaymentMethodDto>> CreatePaymentMethodAsync(int userId, CreatePaymentMethodDto dto)
        {
            var digits = PaymentMethodValidator.NormalizeCardNumber(dto.CardNumber);
            if (digits == null)
                return InvalidCardNumber();

            if (PaymentMethodValidator.IsExpired(dto.ExpiryMonth, dto.ExpiryYear, DateTime.UtcNow))
                return CardExpired();

            if (dto.IsDefault)
                await ClearDefaultAsync(userId, exceptId: null);

            var paymentMethod = new PaymentMethod
            {
                UserId = userId,
                Type = dto.Type,
                CardHolderName = dto.CardHolderName,
                CardNumber = PaymentMethodValidator.MaskCardNumber(digits),
                ExpiryMonth = dto.ExpiryMonth,
                ExpiryYear = dto.ExpiryYear,
                BankName = dto.BankName,
                AccountNumber = PaymentMethodValidator.MaskAccountNumber(dto.AccountNumber),
                AccountHolderName = dto.AccountHolderName,
                IsDefault = dto.IsDefault,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();

            return BaseResponseDto<PaymentMethodDto>.SuccessResult("Payment method created successfully", ToDto(paymentMethod));
        }

        /// <summary>
        /// <c>cardNumber</c> maskeli (<c>*</c> içeren) gelirse mevcut numara korunur; hesap numarası için de aynı kural.
        /// </summary>
        public async Task<BaseResponseDto<PaymentMethodDto>> UpdatePaymentMethodAsync(int paymentMethodId, int userId, UpdatePaymentMethodDto dto)
        {
            var paymentMethod = await FindOwnedAsync(paymentMethodId, userId);
            if (paymentMethod == null)
                return PaymentMethodNotFound<PaymentMethodDto>();

            string? newMaskedCard = null;
            if (!PaymentMethodValidator.IsMasked(dto.CardNumber))
            {
                var digits = PaymentMethodValidator.NormalizeCardNumber(dto.CardNumber);
                if (digits == null)
                    return InvalidCardNumber();
                newMaskedCard = PaymentMethodValidator.MaskCardNumber(digits);
            }

            if (PaymentMethodValidator.IsExpired(dto.ExpiryMonth, dto.ExpiryYear, DateTime.UtcNow))
                return CardExpired();

            if (dto.IsDefault)
                await ClearDefaultAsync(userId, exceptId: paymentMethodId);

            paymentMethod.Type = dto.Type;
            paymentMethod.CardHolderName = dto.CardHolderName;
            if (newMaskedCard != null)
                paymentMethod.CardNumber = newMaskedCard;
            paymentMethod.ExpiryMonth = dto.ExpiryMonth;
            paymentMethod.ExpiryYear = dto.ExpiryYear;
            paymentMethod.BankName = dto.BankName;
            if (!PaymentMethodValidator.IsMasked(dto.AccountNumber))
                paymentMethod.AccountNumber = PaymentMethodValidator.MaskAccountNumber(dto.AccountNumber);
            paymentMethod.AccountHolderName = dto.AccountHolderName;
            paymentMethod.IsDefault = dto.IsDefault;
            paymentMethod.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return BaseResponseDto<PaymentMethodDto>.SuccessResult("Payment method updated successfully", ToDto(paymentMethod));
        }

        public async Task<BaseResponseDto<string>> DeletePaymentMethodAsync(int paymentMethodId, int userId)
        {
            var paymentMethod = await FindOwnedAsync(paymentMethodId, userId);
            if (paymentMethod == null)
                return PaymentMethodNotFound<string>();

            paymentMethod.IsActive = false;
            paymentMethod.IsDefault = false;
            paymentMethod.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<string>.SuccessResult("Payment method deleted successfully", "Payment method deleted successfully");
        }

        public async Task<BaseResponseDto<PaymentMethodDto>> SetDefaultPaymentMethodAsync(int paymentMethodId, int userId)
        {
            var paymentMethod = await FindOwnedAsync(paymentMethodId, userId);
            if (paymentMethod == null)
                return PaymentMethodNotFound<PaymentMethodDto>();

            await ClearDefaultAsync(userId, exceptId: paymentMethodId);
            paymentMethod.IsDefault = true;
            paymentMethod.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<PaymentMethodDto>.SuccessResult("Default payment method set successfully", ToDto(paymentMethod));
        }

        /// <summary>Maskeli görünüm; eski hash kayıtları <c>**** **** **** ****</c> görünür.</summary>
        public static PaymentMethodDto ToDto(PaymentMethod pm) => new()
        {
            Id = pm.Id,
            UserId = pm.UserId,
            Type = pm.Type,
            CardHolderName = pm.CardHolderName,
            CardNumber = PaymentMethodValidator.DisplayCardNumber(pm.CardNumber),
            ExpiryMonth = pm.ExpiryMonth,
            ExpiryYear = pm.ExpiryYear,
            BankName = pm.BankName,
            AccountNumber = PaymentMethodValidator.DisplayAccountNumber(pm.AccountNumber),
            AccountHolderName = pm.AccountHolderName,
            IsDefault = pm.IsDefault,
            IsActive = pm.IsActive,
            CreatedAt = pm.CreatedAt,
            UpdatedAt = pm.UpdatedAt
        };

        private Task<PaymentMethod?> FindOwnedAsync(int paymentMethodId, int userId) =>
            _context.PaymentMethods.FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.UserId == userId && pm.IsActive);

        private async Task ClearDefaultAsync(int userId, int? exceptId)
        {
            var defaults = await _context.PaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive && (exceptId == null || pm.Id != exceptId))
                .ToListAsync();

            foreach (var existing in defaults)
            {
                existing.IsDefault = false;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static BaseResponseDto<T> PaymentMethodNotFound<T>() =>
            BaseResponseDto<T>.NotFound("Payment method not found", ErrorCodes.PaymentMethodNotFound);

        private static BaseResponseDto<PaymentMethodDto> InvalidCardNumber() =>
            BaseResponseDto<PaymentMethodDto>.Fail("Geçersiz kart numarası.", ErrorCodes.InvalidCardNumber);

        private static BaseResponseDto<PaymentMethodDto> CardExpired() =>
            BaseResponseDto<PaymentMethodDto>.Fail("Kartın son kullanma tarihi geçmiş.", ErrorCodes.CardExpired);
    }
}
