using System.Security.Claims;

namespace EcommerceBackend.Application.Services
{
    public interface IJwtService
    {
        /// <param name="role">JWT role claim (ör. User, Admin).</param>
        string GenerateToken(string email, long userId, string role = "User");
        ClaimsPrincipal? ValidateToken(string token);
    }
}
