using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IPaymentMethodService
    {
        Task<BaseResponseDto<List<PaymentMethodDto>>> GetUserPaymentMethodsAsync(int userId);
        Task<BaseResponseDto<PaymentMethodDto>> GetPaymentMethodByIdAsync(int paymentMethodId, int userId);
        Task<BaseResponseDto<PaymentMethodDto>> CreatePaymentMethodAsync(int userId, CreatePaymentMethodDto createPaymentMethodDto);
        Task<BaseResponseDto<PaymentMethodDto>> UpdatePaymentMethodAsync(int paymentMethodId, int userId, UpdatePaymentMethodDto updatePaymentMethodDto);
        Task<BaseResponseDto<string>> DeletePaymentMethodAsync(int paymentMethodId, int userId);
        Task<BaseResponseDto<PaymentMethodDto>> SetDefaultPaymentMethodAsync(int paymentMethodId, int userId);
    }
}
