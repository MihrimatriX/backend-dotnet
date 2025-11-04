using System.Security.Claims;

namespace EcommerceBackend.Application.Services
{
    public interface IJwtService
    {
        string GenerateToken(string email, long userId);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
