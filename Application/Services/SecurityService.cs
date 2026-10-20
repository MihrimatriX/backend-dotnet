using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Application.Services
{
    public class SecurityService : ISecurityService
    {
        private const int PasswordWorkFactor = 12;

        private readonly ApplicationDbContext _context;

        public SecurityService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary><c>lastLoginAt/lastLoginIp</c> son başarılı girişten; <c>recentLogins</c> en yeni 5 kayıt.</summary>
        public async Task<BaseResponseDto<SecurityDto>> GetSecurityInfoAsync(int userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            if (user == null)
                return UserNotFound<SecurityDto>();

            var lastLogin = await ActiveLogins(userId)
                .Where(lh => lh.IsSuccessful)
                .OrderByDescending(lh => lh.LoginAt)
                .ThenByDescending(lh => lh.Id)
                .Select(lh => new { lh.LoginAt, lh.IpAddress })
                .FirstOrDefaultAsync();

            var recentLogins = await ActiveLogins(userId)
                .OrderByDescending(lh => lh.LoginAt)
                .ThenByDescending(lh => lh.Id)
                .Take(5)
                .ToListAsync();

            var securityInfo = new SecurityDto
            {
                UserId = user.Id,
                Email = user.Email,
                IsEmailVerified = user.IsEmailVerified,
                LastPasswordChange = user.UpdatedAt,
                TwoFactorEnabled = false,
                LastLoginAt = lastLogin?.LoginAt,
                LastLoginIp = lastLogin?.IpAddress,
                RecentLogins = recentLogins.Select(ToDto).ToList()
            };

            return BaseResponseDto<SecurityDto>.SuccessResult("Security information retrieved successfully", securityInfo);
        }

        /// <summary>Şifre değişince çağıran token hariç eski token'lar iptal edilir (§5.6).</summary>
        public async Task<BaseResponseDto<string>> ChangePasswordAsync(int userId, string? currentTokenId, ChangePasswordDto changePasswordDto)
        {
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
                return BaseResponseDto<string>.Fail("New password and confirm password do not match", ErrorCodes.ValidationError);

            var user = await FindActiveUserAsync(userId);
            if (user == null)
                return UserNotFound<string>();

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
                return BaseResponseDto<string>.Fail("Current password is incorrect", ErrorCodes.InvalidPassword);

            user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword, PasswordWorkFactor);
            RevokeTokens(user, currentTokenId);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<string>.SuccessResult("Password changed successfully", "Password changed successfully");
        }

        /// <summary>E-posta kırpılıp küçük harfe çevrilir; benzersizlik büyük/küçük harf duyarsızdır.</summary>
        public async Task<BaseResponseDto<string>> UpdateEmailAsync(int userId, UpdateEmailDto updateEmailDto)
        {
            var user = await FindActiveUserAsync(userId);
            if (user == null)
                return UserNotFound<string>();

            if (!BCrypt.Net.BCrypt.Verify(updateEmailDto.CurrentPassword, user.Password))
                return BaseResponseDto<string>.Fail("Current password is incorrect", ErrorCodes.InvalidPassword);

            var newEmail = AuthService.NormalizeEmail(updateEmailDto.NewEmail);
            var taken = await _context.Users.AnyAsync(u => u.Id != userId && u.Email.ToLower() == newEmail);
            if (taken)
                return BaseResponseDto<string>.Fail("Email address is already in use", ErrorCodes.EmailTaken);

            user.Email = newEmail;
            user.IsEmailVerified = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<string>.SuccessResult("Email updated successfully", "Email updated successfully");
        }

        /// <summary>Taslak: yalnızca şifreyi doğrular, 2FA henüz uygulanmadı.</summary>
        public async Task<BaseResponseDto<string>> EnableTwoFactorAsync(int userId, EnableTwoFactorDto enableTwoFactorDto)
        {
            var check = await VerifyPasswordAsync<string>(userId, enableTwoFactorDto.Password);
            return check ?? BaseResponseDto<string>.SuccessResult(
                "Two-factor authentication enabled successfully", "Two-factor authentication enabled successfully");
        }

        /// <summary>Taslak: yalnızca şifreyi doğrular, doğrulama kodu kontrol edilmez.</summary>
        public async Task<BaseResponseDto<string>> DisableTwoFactorAsync(int userId, DisableTwoFactorDto disableTwoFactorDto)
        {
            var check = await VerifyPasswordAsync<string>(userId, disableTwoFactorDto.Password);
            return check ?? BaseResponseDto<string>.SuccessResult(
                "Two-factor authentication disabled successfully", "Two-factor authentication disabled successfully");
        }

        public async Task<BaseResponseDto<List<LoginHistoryDto>>> GetLoginHistoryAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var paging = Paging.Normalize(pageNumber, pageSize);

            var loginHistory = await ActiveLogins(userId)
                .OrderByDescending(lh => lh.LoginAt)
                .ThenByDescending(lh => lh.Id)
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .ToListAsync();

            return BaseResponseDto<List<LoginHistoryDto>>.SuccessResult(
                "Login history retrieved successfully",
                loginHistory.Select(ToDto).ToList());
        }

        /// <summary>Kayıt yoksa varsayılanlar döner (kayıt oluşturulmaz).</summary>
        public async Task<BaseResponseDto<SecuritySettingsDto>> GetSecuritySettingsAsync(int userId)
        {
            var settings = await _context.SecuritySettings.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);

            return BaseResponseDto<SecuritySettingsDto>.SuccessResult(
                "Security settings retrieved successfully",
                settings == null ? new SecuritySettingsDto() : ToDto(settings));
        }

        /// <summary>Upsert: kullanıcının kaydı yoksa oluşturulur.</summary>
        public async Task<BaseResponseDto<SecuritySettingsDto>> UpdateSecuritySettingsAsync(int userId, SecuritySettingsDto securitySettingsDto)
        {
            var settings = await _context.SecuritySettings.FirstOrDefaultAsync(s => s.UserId == userId);
            if (settings == null)
            {
                settings = new SecuritySettings { UserId = userId, CreatedAt = DateTime.UtcNow };
                _context.SecuritySettings.Add(settings);
            }

            settings.EmailNotifications = securitySettingsDto.EmailNotifications;
            settings.SmsNotifications = securitySettingsDto.SmsNotifications;
            settings.LoginAlerts = securitySettingsDto.LoginAlerts;
            settings.TwoFactorRequired = securitySettingsDto.TwoFactorRequired;
            settings.SessionTimeout = securitySettingsDto.SessionTimeout;
            settings.IsActive = true;
            settings.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return BaseResponseDto<SecuritySettingsDto>.SuccessResult("Security settings updated successfully", ToDto(settings));
        }

        public async Task<BaseResponseDto<string>> LogoutAllDevicesAsync(int userId, string? currentTokenId)
        {
            var user = await FindActiveUserAsync(userId);
            if (user == null)
                return UserNotFound<string>();

            RevokeTokens(user, currentTokenId);
            await _context.SaveChangesAsync();

            return BaseResponseDto<string>.SuccessResult("All devices logged out successfully", "All devices logged out successfully");
        }

        /// <summary>
        /// §5.6: iptal zamanı saniyeye kırpılır; <c>iat</c> bu zamana eşit/eski token'lar reddedilir,
        /// <paramref name="exceptTokenId"/> hariç.
        /// </summary>
        private static void RevokeTokens(User user, string? exceptTokenId)
        {
            var now = DateTime.UtcNow;
            user.TokensRevokedAt = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);
            user.RevokeExceptJti = exceptTokenId;
        }

        /// <summary>Şifre doğruysa null, değilse hata zarfı.</summary>
        private async Task<BaseResponseDto<T>?> VerifyPasswordAsync<T>(int userId, string password)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            if (user == null)
                return UserNotFound<T>();

            return BCrypt.Net.BCrypt.Verify(password, user.Password)
                ? null
                : BaseResponseDto<T>.Fail("Password is incorrect", ErrorCodes.InvalidPassword);
        }

        private Task<User?> FindActiveUserAsync(int userId) =>
            _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        private IQueryable<LoginHistory> ActiveLogins(int userId) =>
            _context.LoginHistories.AsNoTracking().Where(lh => lh.UserId == userId && lh.IsActive);

        private static LoginHistoryDto ToDto(LoginHistory lh) => new()
        {
            Id = lh.Id,
            LoginAt = lh.LoginAt,
            IpAddress = lh.IpAddress,
            UserAgent = lh.UserAgent,
            Location = lh.Location,
            IsSuccessful = lh.IsSuccessful
        };

        private static SecuritySettingsDto ToDto(SecuritySettings s) => new()
        {
            EmailNotifications = s.EmailNotifications,
            SmsNotifications = s.SmsNotifications,
            LoginAlerts = s.LoginAlerts,
            TwoFactorRequired = s.TwoFactorRequired,
            SessionTimeout = s.SessionTimeout
        };

        private static BaseResponseDto<T> UserNotFound<T>() =>
            BaseResponseDto<T>.NotFound("User not found", ErrorCodes.UserNotFound);
    }
}
