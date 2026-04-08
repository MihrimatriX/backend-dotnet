using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Services
{
    public interface IAuthService
    {
        Task<BaseResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto loginRequest);
        Task<BaseResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto registerRequest);
        Task<BaseResponseDto<string>> LogoutAsync();
    }
}
