using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EcommerceBackend.Application.Services
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "Invalid email or password";

        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly AuthOptions _authOptions;

        public AuthService(ApplicationDbContext context, IJwtService jwtService, IOptions<AuthOptions> authOptions)
        {
            _context = context;
            _jwtService = jwtService;
            _authOptions = authOptions.Value;
        }

        /// <summary>E-postalar kırpılıp küçük harfe çevrilerek saklanır ve karşılaştırılır (§2).</summary>
        public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        public async Task<BaseResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto loginRequest, ClientInfo client)
        {
            var email = NormalizeEmail(loginRequest.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null)
                return InvalidCredentials();

            if (!user.IsActive || !VerifyPassword(loginRequest.Password, user.Password))
            {
                RecordLogin(user.Id, client, failureReason: user.IsActive ? "Invalid password" : "Inactive account");
                await _context.SaveChangesAsync();
                return InvalidCredentials();
            }

            RecordLogin(user.Id, client, failureReason: null);
            await _context.SaveChangesAsync();

            return BaseResponseDto<AuthResponseDto>.SuccessResult("Login successful", CreateAuthResponse(user));
        }

        public async Task<BaseResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto registerRequest)
        {
            var email = NormalizeEmail(registerRequest.Email);
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
                return BaseResponseDto<AuthResponseDto>.Fail("Email is already taken", ErrorCodes.EmailTaken, 409);

            var user = new User
            {
                Email = email,
                Password = HashPassword(registerRequest.Password),
                FirstName = registerRequest.FirstName.Trim(),
                LastName = registerRequest.LastName.Trim(),
                PhoneNumber = registerRequest.PhoneNumber,
                Address = registerRequest.Address,
                City = registerRequest.City,
                PostalCode = registerRequest.PostalCode,
                IsEmailVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return BaseResponseDto<AuthResponseDto>.SuccessResult("User registered successfully", CreateAuthResponse(user));
        }

        public Task<BaseResponseDto<string>> LogoutAsync()
        {
            // JWT durumsuz; istemci token'ı siler. Tüm cihazlardan çıkış için /api/security/logout-all-devices.
            return Task.FromResult(BaseResponseDto<string>.SuccessResult("User logged out successfully", "Logout successful"));
        }

        private AuthResponseDto CreateAuthResponse(User user)
        {
            var role = _authOptions.ResolveRole(NormalizeEmail(user.Email));
            return new AuthResponseDto
            {
                Token = _jwtService.GenerateToken(user.Email, user.Id, role),
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsEmailVerified = user.IsEmailVerified,
                Role = role,
            };
        }

        private static BaseResponseDto<AuthResponseDto> InvalidCredentials() =>
            BaseResponseDto<AuthResponseDto>.Fail(InvalidCredentialsMessage, ErrorCodes.InvalidCredentials, 401);

        private void RecordLogin(int userId, ClientInfo client, string? failureReason)
        {
            _context.LoginHistories.Add(new LoginHistory
            {
                UserId = userId,
                LoginAt = DateTime.UtcNow,
                IpAddress = Truncate(client.IpAddress, 45),
                UserAgent = Truncate(client.UserAgent, 500),
                IsSuccessful = failureReason == null,
                FailureReason = failureReason,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
        }

        private static string? Truncate(string? value, int maxLength) =>
            string.IsNullOrEmpty(value) || value.Length <= maxLength ? value : value[..maxLength];

        public static string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(10));

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }
    }
}
