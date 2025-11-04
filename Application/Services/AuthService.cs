using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace EcommerceBackend.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<BaseResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(loginRequest.Email);
                if (user == null)
                {
                    return BaseResponseDto<AuthResponseDto>.ErrorResult("Invalid email or password");
                }

                if (!VerifyPassword(loginRequest.Password, user.Password))
                {
                    return BaseResponseDto<AuthResponseDto>.ErrorResult("Invalid email or password");
                }

                var token = _jwtService.GenerateToken(user.Email, user.Id);

                var authResponse = new AuthResponseDto
                {
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsEmailVerified = user.IsEmailVerified
                };

                return BaseResponseDto<AuthResponseDto>.SuccessResult("Login successful", authResponse);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<AuthResponseDto>.ErrorResult($"Login failed: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto registerRequest)
        {
            try
            {
                if (await _userRepository.ExistsByEmailAsync(registerRequest.Email))
                {
                    return BaseResponseDto<AuthResponseDto>.ErrorResult("Email is already taken");
                }

                var user = new User
                {
                    Email = registerRequest.Email,
                    Password = HashPassword(registerRequest.Password),
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    PhoneNumber = registerRequest.PhoneNumber,
                    Address = registerRequest.Address,
                    City = registerRequest.City,
                    PostalCode = registerRequest.PostalCode,
                    IsEmailVerified = false,
                    IsActive = true
                };

                var createdUser = await _userRepository.CreateAsync(user);
                var token = _jwtService.GenerateToken(createdUser.Email, createdUser.Id);

                var authResponse = new AuthResponseDto
                {
                    Token = token,
                    UserId = createdUser.Id,
                    Email = createdUser.Email,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    IsEmailVerified = createdUser.IsEmailVerified
                };

                return BaseResponseDto<AuthResponseDto>.SuccessResult("User registered successfully", authResponse);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<AuthResponseDto>.ErrorResult($"Registration failed: {ex.Message}");
            }
        }

        public Task<BaseResponseDto<string>> LogoutAsync()
        {
            // JWT is stateless, so logout is handled on client side
            return Task.FromResult(BaseResponseDto<string>.SuccessResult("User logged out successfully", "Logout successful"));
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(10));
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password verification error: {ex.Message}");
                return false;
            }
        }
    }
}
