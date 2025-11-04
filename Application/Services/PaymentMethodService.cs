using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
            try
            {
                var paymentMethods = await _context.PaymentMethods
                    .Where(pm => pm.UserId == userId && pm.IsActive)
                    .OrderByDescending(pm => pm.IsDefault)
                    .ThenByDescending(pm => pm.CreatedAt)
                    .Select(pm => new PaymentMethodDto
                    {
                        Id = pm.Id,
                        UserId = pm.UserId,
                        Type = pm.Type,
                        CardHolderName = pm.CardHolderName,
                        CardNumber = MaskCardNumber(pm.CardNumber),
                        ExpiryMonth = pm.ExpiryMonth,
                        ExpiryYear = pm.ExpiryYear,
                        BankName = pm.BankName,
                        AccountNumber = pm.AccountNumber != null ? MaskAccountNumber(pm.AccountNumber) : null,
                        AccountHolderName = pm.AccountHolderName,
                        IsDefault = pm.IsDefault,
                        IsActive = pm.IsActive,
                        CreatedAt = pm.CreatedAt,
                        UpdatedAt = pm.UpdatedAt
                    })
                    .ToListAsync();

                return BaseResponseDto<List<PaymentMethodDto>>.SuccessResult("Payment methods retrieved successfully", paymentMethods);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<List<PaymentMethodDto>>.ErrorResult($"Error retrieving payment methods: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<PaymentMethodDto>> GetPaymentMethodByIdAsync(int paymentMethodId, int userId)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .Where(pm => pm.Id == paymentMethodId && pm.UserId == userId && pm.IsActive)
                    .Select(pm => new PaymentMethodDto
                    {
                        Id = pm.Id,
                        UserId = pm.UserId,
                        Type = pm.Type,
                        CardHolderName = pm.CardHolderName,
                        CardNumber = MaskCardNumber(pm.CardNumber),
                        ExpiryMonth = pm.ExpiryMonth,
                        ExpiryYear = pm.ExpiryYear,
                        BankName = pm.BankName,
                        AccountNumber = pm.AccountNumber != null ? MaskAccountNumber(pm.AccountNumber) : null,
                        AccountHolderName = pm.AccountHolderName,
                        IsDefault = pm.IsDefault,
                        IsActive = pm.IsActive,
                        CreatedAt = pm.CreatedAt,
                        UpdatedAt = pm.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (paymentMethod == null)
                {
                    return BaseResponseDto<PaymentMethodDto>.ErrorResult("Payment method not found");
                }

                return BaseResponseDto<PaymentMethodDto>.SuccessResult("Payment method retrieved successfully", paymentMethod);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<PaymentMethodDto>.ErrorResult($"Error retrieving payment method: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<PaymentMethodDto>> CreatePaymentMethodAsync(int userId, CreatePaymentMethodDto createPaymentMethodDto)
        {
            try
            {
                // If this is set as default, remove default from other payment methods
                if (createPaymentMethodDto.IsDefault)
                {
                    var existingDefaultPaymentMethods = await _context.PaymentMethods
                        .Where(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive)
                        .ToListAsync();

                    foreach (var existingPaymentMethod in existingDefaultPaymentMethods)
                    {
                        existingPaymentMethod.IsDefault = false;
                        existingPaymentMethod.UpdatedAt = DateTime.UtcNow;
                    }
                }

                var paymentMethod = new PaymentMethod
                {
                    UserId = userId,
                    Type = createPaymentMethodDto.Type,
                    CardHolderName = createPaymentMethodDto.CardHolderName,
                    CardNumber = EncryptCardNumber(createPaymentMethodDto.CardNumber),
                    ExpiryMonth = createPaymentMethodDto.ExpiryMonth,
                    ExpiryYear = createPaymentMethodDto.ExpiryYear,
                    Cvv = createPaymentMethodDto.Cvv != null ? EncryptCvv(createPaymentMethodDto.Cvv) : null,
                    BankName = createPaymentMethodDto.BankName,
                    AccountNumber = createPaymentMethodDto.AccountNumber != null ? EncryptAccountNumber(createPaymentMethodDto.AccountNumber) : null,
                    AccountHolderName = createPaymentMethodDto.AccountHolderName,
                    IsDefault = createPaymentMethodDto.IsDefault,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PaymentMethods.Add(paymentMethod);
                await _context.SaveChangesAsync();

                var paymentMethodDto = new PaymentMethodDto
                {
                    Id = paymentMethod.Id,
                    UserId = paymentMethod.UserId,
                    Type = paymentMethod.Type,
                    CardHolderName = paymentMethod.CardHolderName,
                    CardNumber = MaskCardNumber(paymentMethod.CardNumber),
                    ExpiryMonth = paymentMethod.ExpiryMonth,
                    ExpiryYear = paymentMethod.ExpiryYear,
                    BankName = paymentMethod.BankName,
                    AccountNumber = paymentMethod.AccountNumber != null ? MaskAccountNumber(paymentMethod.AccountNumber) : null,
                    AccountHolderName = paymentMethod.AccountHolderName,
                    IsDefault = paymentMethod.IsDefault,
                    IsActive = paymentMethod.IsActive,
                    CreatedAt = paymentMethod.CreatedAt,
                    UpdatedAt = paymentMethod.UpdatedAt
                };

                return BaseResponseDto<PaymentMethodDto>.SuccessResult("Payment method created successfully", paymentMethodDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<PaymentMethodDto>.ErrorResult($"Error creating payment method: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<PaymentMethodDto>> UpdatePaymentMethodAsync(int paymentMethodId, int userId, UpdatePaymentMethodDto updatePaymentMethodDto)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .Where(pm => pm.Id == paymentMethodId && pm.UserId == userId && pm.IsActive)
                    .FirstOrDefaultAsync();

                if (paymentMethod == null)
                {
                    return BaseResponseDto<PaymentMethodDto>.ErrorResult("Payment method not found");
                }

                // If this is set as default, remove default from other payment methods
                if (updatePaymentMethodDto.IsDefault && !paymentMethod.IsDefault)
                {
                    var existingDefaultPaymentMethods = await _context.PaymentMethods
                        .Where(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive && pm.Id != paymentMethodId)
                        .ToListAsync();

                    foreach (var existingPaymentMethod in existingDefaultPaymentMethods)
                    {
                        existingPaymentMethod.IsDefault = false;
                        existingPaymentMethod.UpdatedAt = DateTime.UtcNow;
                    }
                }

                paymentMethod.Type = updatePaymentMethodDto.Type;
                paymentMethod.CardHolderName = updatePaymentMethodDto.CardHolderName;
                paymentMethod.CardNumber = EncryptCardNumber(updatePaymentMethodDto.CardNumber);
                paymentMethod.ExpiryMonth = updatePaymentMethodDto.ExpiryMonth;
                paymentMethod.ExpiryYear = updatePaymentMethodDto.ExpiryYear;
                paymentMethod.Cvv = updatePaymentMethodDto.Cvv != null ? EncryptCvv(updatePaymentMethodDto.Cvv) : null;
                paymentMethod.BankName = updatePaymentMethodDto.BankName;
                paymentMethod.AccountNumber = updatePaymentMethodDto.AccountNumber != null ? EncryptAccountNumber(updatePaymentMethodDto.AccountNumber) : null;
                paymentMethod.AccountHolderName = updatePaymentMethodDto.AccountHolderName;
                paymentMethod.IsDefault = updatePaymentMethodDto.IsDefault;
                paymentMethod.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var paymentMethodDto = new PaymentMethodDto
                {
                    Id = paymentMethod.Id,
                    UserId = paymentMethod.UserId,
                    Type = paymentMethod.Type,
                    CardHolderName = paymentMethod.CardHolderName,
                    CardNumber = MaskCardNumber(paymentMethod.CardNumber),
                    ExpiryMonth = paymentMethod.ExpiryMonth,
                    ExpiryYear = paymentMethod.ExpiryYear,
                    BankName = paymentMethod.BankName,
                    AccountNumber = paymentMethod.AccountNumber != null ? MaskAccountNumber(paymentMethod.AccountNumber) : null,
                    AccountHolderName = paymentMethod.AccountHolderName,
                    IsDefault = paymentMethod.IsDefault,
                    IsActive = paymentMethod.IsActive,
                    CreatedAt = paymentMethod.CreatedAt,
                    UpdatedAt = paymentMethod.UpdatedAt
                };

                return BaseResponseDto<PaymentMethodDto>.SuccessResult("Payment method updated successfully", paymentMethodDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<PaymentMethodDto>.ErrorResult($"Error updating payment method: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> DeletePaymentMethodAsync(int paymentMethodId, int userId)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .Where(pm => pm.Id == paymentMethodId && pm.UserId == userId && pm.IsActive)
                    .FirstOrDefaultAsync();

                if (paymentMethod == null)
                {
                    return BaseResponseDto<string>.ErrorResult("Payment method not found");
                }

                paymentMethod.IsActive = false;
                paymentMethod.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return BaseResponseDto<string>.SuccessResult("Payment method deleted successfully", "Payment method deleted successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error deleting payment method: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<PaymentMethodDto>> SetDefaultPaymentMethodAsync(int paymentMethodId, int userId)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .Where(pm => pm.Id == paymentMethodId && pm.UserId == userId && pm.IsActive)
                    .FirstOrDefaultAsync();

                if (paymentMethod == null)
                {
                    return BaseResponseDto<PaymentMethodDto>.ErrorResult("Payment method not found");
                }

                // Remove default from other payment methods
                var existingDefaultPaymentMethods = await _context.PaymentMethods
                    .Where(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive && pm.Id != paymentMethodId)
                    .ToListAsync();

                foreach (var existingPaymentMethod in existingDefaultPaymentMethods)
                {
                    existingPaymentMethod.IsDefault = false;
                    existingPaymentMethod.UpdatedAt = DateTime.UtcNow;
                }

                paymentMethod.IsDefault = true;
                paymentMethod.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var paymentMethodDto = new PaymentMethodDto
                {
                    Id = paymentMethod.Id,
                    UserId = paymentMethod.UserId,
                    Type = paymentMethod.Type,
                    CardHolderName = paymentMethod.CardHolderName,
                    CardNumber = MaskCardNumber(paymentMethod.CardNumber),
                    ExpiryMonth = paymentMethod.ExpiryMonth,
                    ExpiryYear = paymentMethod.ExpiryYear,
                    BankName = paymentMethod.BankName,
                    AccountNumber = paymentMethod.AccountNumber != null ? MaskAccountNumber(paymentMethod.AccountNumber) : null,
                    AccountHolderName = paymentMethod.AccountHolderName,
                    IsDefault = paymentMethod.IsDefault,
                    IsActive = paymentMethod.IsActive,
                    CreatedAt = paymentMethod.CreatedAt,
                    UpdatedAt = paymentMethod.UpdatedAt
                };

                return BaseResponseDto<PaymentMethodDto>.SuccessResult("Default payment method set successfully", paymentMethodDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<PaymentMethodDto>.ErrorResult($"Error setting default payment method: {ex.Message}");
            }
        }

        private string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4)
                return cardNumber;

            return "**** **** **** " + cardNumber.Substring(cardNumber.Length - 4);
        }

        private string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 4)
                return accountNumber;

            return "****" + accountNumber.Substring(accountNumber.Length - 4);
        }

        private string EncryptCardNumber(string cardNumber)
        {
            // Simple encryption for demo purposes - in production use proper encryption
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(cardNumber + "salt"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private string EncryptCvv(string cvv)
        {
            // Simple encryption for demo purposes - in production use proper encryption
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(cvv + "salt"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private string EncryptAccountNumber(string accountNumber)
        {
            // Simple encryption for demo purposes - in production use proper encryption
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(accountNumber + "salt"));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
