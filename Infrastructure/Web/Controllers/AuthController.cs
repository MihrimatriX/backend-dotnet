using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Infrastructure.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<BaseResponseDto<AuthResponseDto>>> Register(RegisterRequestDto registerRequest)
        {
            var result = await _authService.RegisterAsync(registerRequest);
            
            if (result.Success)
            {
                return StatusCode(201, result);
            }
            
            if (result.Message.Contains("already taken"))
            {
                return Conflict(result);
            }
            
            return BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<BaseResponseDto<AuthResponseDto>>> Login(LoginRequestDto loginRequest)
        {
            var result = await _authService.LoginAsync(loginRequest);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return Unauthorized(result);
        }

        [HttpPost("logout")]
        public async Task<ActionResult<BaseResponseDto<string>>> Logout()
        {
            var result = await _authService.LogoutAsync();
            return Ok(result);
        }
    }
}
