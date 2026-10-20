using System.Globalization;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EcommerceBackend.Application.Security;

/// <summary>
/// Token'daki kısa claim adları (§2). JwtBearer <c>MapInboundClaims=false</c> ile çalıştığından
/// gelen claim'ler de bu adlarla okunur.
/// </summary>
public static class AuthClaimTypes
{
    public const string Subject = JwtRegisteredClaimNames.Sub;
    public const string Email = JwtRegisteredClaimNames.Email;
    public const string Role = "role";
    public const string TokenId = JwtRegisteredClaimNames.Jti;
    public const string IssuedAt = JwtRegisteredClaimNames.Iat;
}

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirst(AuthClaimTypes.Subject)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
            ? id
            : null;

    public static string? GetTokenId(this ClaimsPrincipal principal) =>
        principal.FindFirst(AuthClaimTypes.TokenId)?.Value;

    /// <summary><c>iat</c> (Unix saniye); yoksa null.</summary>
    public static long? GetIssuedAtUnixSeconds(this ClaimsPrincipal principal) =>
        long.TryParse(principal.FindFirst(AuthClaimTypes.IssuedAt)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var iat)
            ? iat
            : null;
}
