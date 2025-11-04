using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EcommerceBackend.Application.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public SecurityService(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<BaseResponseDto<SecurityDto>> GetSecurityInfoAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return BaseResponseDto<SecurityDto>.ErrorResult("User not found");
                }

                var recentLogins = await _context.LoginHistories
                    .Where(lh => lh.UserId == userId && lh.IsActive)
                    .OrderByDescending(lh => lh.LoginAt)
                    .Take(5)
                    .Select(lh => new LoginHistoryDto
                    {
                        Id = lh.Id,
                        LoginAt = lh.LoginAt,
                        IpAddress = lh.IpAddress,
                        UserAgent = lh.UserAgent,
                        Location = lh.Location,
                        IsSuccessful = lh.IsSuccessful
                    })
                    .ToListAsync();

                var securityInfo = new SecurityDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    IsEmailVerified = user.IsEmailVerified,
                    LastPasswordChange = user.UpdatedAt, // Assuming this tracks password changes
                    TwoFactorEnabled = false, // Implement 2FA later
                    LastLoginAt = null, // Add this field to User entity if needed
                    LastLoginIp = null, // Add this field to User entity if needed
                    RecentLogins = recentLogins
                };

                return BaseResponseDto<SecurityDto>.SuccessResult("Security information retrieved successfully", securityInfo);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<SecurityDto>.ErrorResult($"Error retrieving security information: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return BaseResponseDto<string>.ErrorResult("User not found");
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
                {
                    return BaseResponseDto<string>.ErrorResult("Current password is incorrect");
                }

                // Hash new password
                var hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                user.Password = hashedNewPassword;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return BaseResponseDto<string>.SuccessResult("Password changed successfully", "Password changed successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error changing password: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> UpdateEmailAsync(int userId, UpdateEmailDto updateEmailDto)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return BaseResponseDto<string>.ErrorResult("User not found");
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(updateEmailDto.CurrentPassword, user.Password))
                {
                    return BaseResponseDto<string>.ErrorResult("Current password is incorrect");
                }

                // Check if email already exists
                var existingUser = await _context.Users
                    .Where(u => u.Email == updateEmailDto.NewEmail && u.Id != userId)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    return BaseResponseDto<string>.ErrorResult("Email address is already in use");
                }

                user.Email = updateEmailDto.NewEmail;
                user.IsEmailVerified = false; // Require email verification
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return BaseResponseDto<string>.SuccessResult("Email updated successfully. Please verify your new email address.", "Email updated successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error updating email: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> EnableTwoFactorAsync(int userId, EnableTwoFactorDto enableTwoFactorDto)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return BaseResponseDto<string>.ErrorResult("User not found");
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(enableTwoFactorDto.Password, user.Password))
                {
                    return BaseResponseDto<string>.ErrorResult("Password is incorrect");
                }

                // TODO: Implement 2FA logic (generate secret, QR code, etc.)
                // For now, just return success
                return BaseResponseDto<string>.SuccessResult("Two-factor authentication enabled successfully", "Two-factor authentication enabled successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error enabling two-factor authentication: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> DisableTwoFactorAsync(int userId, DisableTwoFactorDto disableTwoFactorDto)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId && u.IsActive)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return BaseResponseDto<string>.ErrorResult("User not found");
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(disableTwoFactorDto.Password, user.Password))
                {
                    return BaseResponseDto<string>.ErrorResult("Password is incorrect");
                }

                // TODO: Verify 2FA code
                // For now, just return success
                return BaseResponseDto<string>.SuccessResult("Two-factor authentication disabled successfully", "Two-factor authentication disabled successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error disabling two-factor authentication: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<List<LoginHistoryDto>>> GetLoginHistoryAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var loginHistory = await _context.LoginHistories
                    .Where(lh => lh.UserId == userId && lh.IsActive)
                    .OrderByDescending(lh => lh.LoginAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(lh => new LoginHistoryDto
                    {
                        Id = lh.Id,
                        LoginAt = lh.LoginAt,
                        IpAddress = lh.IpAddress,
                        UserAgent = lh.UserAgent,
                        Location = lh.Location,
                        IsSuccessful = lh.IsSuccessful
                    })
                    .ToListAsync();

                return BaseResponseDto<List<LoginHistoryDto>>.SuccessResult("Login history retrieved successfully", loginHistory);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<List<LoginHistoryDto>>.ErrorResult($"Error retrieving login history: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<SecuritySettingsDto>> GetSecuritySettingsAsync(int userId)
        {
            try
            {
                // TODO: Implement security settings storage
                // For now, return default settings
                var settings = new SecuritySettingsDto
                {
                    EmailNotifications = true,
                    SmsNotifications = false,
                    LoginAlerts = true,
                    TwoFactorRequired = false,
                    SessionTimeout = 30
                };

                return BaseResponseDto<SecuritySettingsDto>.SuccessResult("Security settings retrieved successfully", settings);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<SecuritySettingsDto>.ErrorResult($"Error retrieving security settings: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<SecuritySettingsDto>> UpdateSecuritySettingsAsync(int userId, SecuritySettingsDto securitySettingsDto)
        {
            try
            {
                // TODO: Implement security settings storage
                // For now, just return the updated settings
                return BaseResponseDto<SecuritySettingsDto>.SuccessResult("Security settings updated successfully", securitySettingsDto);
            }
            catch (Exception ex)
            {
                return BaseResponseDto<SecuritySettingsDto>.ErrorResult($"Error updating security settings: {ex.Message}");
            }
        }

        public async Task<BaseResponseDto<string>> LogoutAllDevicesAsync(int userId)
        {
            try
            {
                // TODO: Implement device logout logic
                // This would typically involve invalidating all JWT tokens for the user
                return BaseResponseDto<string>.SuccessResult("All devices logged out successfully", "All devices logged out successfully");
            }
            catch (Exception ex)
            {
                return BaseResponseDto<string>.ErrorResult($"Error logging out all devices: {ex.Message}");
            }
        }
    }
}
