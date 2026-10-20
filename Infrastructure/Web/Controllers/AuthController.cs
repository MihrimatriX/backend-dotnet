using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<BaseResponseDto<AuthResponseDto>>> Register(RegisterRequestDto registerRequest) =>
            RespondCreated(await _authService.RegisterAsync(registerRequest));

        [HttpPost("login")]
        public async Task<ActionResult<BaseResponseDto<AuthResponseDto>>> Login(LoginRequestDto loginRequest)
        {
            var client = new ClientInfo(
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers[HeaderNames.UserAgent].ToString());
            return Respond(await _authService.LoginAsync(loginRequest, client));
        }

        [HttpPost("logout")]
        public async Task<ActionResult<BaseResponseDto<string>>> Logout() =>
            Respond(await _authService.LogoutAsync());
    }
}
