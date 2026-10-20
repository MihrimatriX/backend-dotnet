using System.Text;
using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Security;
using EcommerceBackend.Application.Serialization;
using EcommerceBackend.Infrastructure.Data;
using EcommerceBackend.Infrastructure.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EcommerceBackend.Infrastructure.Security;

/// <summary>
/// JWT Bearer doğrulaması (§2): kısa claim adları (<c>sub</c>, <c>role</c>), pasif kullanıcı ve
/// <c>logout-all-devices</c> iptal kontrolü (§5.6), 401/403 için JSON zarf.
/// </summary>
public static class JwtAuthenticationSetup
{
    public static AuthenticationBuilder AddApiJwtAuthentication(this IServiceCollection services, JwtOptions jwt) =>
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    NameClaimType = AuthClaimTypes.Subject,
                    RoleClaimType = AuthClaimTypes.Role,
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ValidateTokenStateAsync,
                    OnChallenge = WriteUnauthorizedAsync,
                    OnForbidden = context => ApiErrorResponse.WriteAsync(
                        context.HttpContext,
                        StatusCodes.Status403Forbidden,
                        ErrorCodes.Forbidden,
                        ErrorMessages.Forbidden),
                };
            });

    /// <summary>
    /// Kullanıcı hâlâ aktif mi ve token iptal edilmiş mi: <c>iat ≤ tokensRevokedAt</c> ve <c>jti ≠ revokeExceptJti</c> → red.
    /// </summary>
    private static async Task ValidateTokenStateAsync(TokenValidatedContext context)
    {
        var principal = context.Principal;
        var userId = principal?.GetUserId();
        if (principal is null || userId is null)
        {
            context.Fail("Token does not contain a valid subject.");
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        var state = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.IsActive, u.TokensRevokedAt, u.RevokeExceptJti })
            .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

        if (state is null || !state.IsActive)
        {
            context.Fail("User is missing or inactive.");
            return;
        }

        if (state.TokensRevokedAt is not { } revokedAt)
            return;

        var revokedAtSeconds = new DateTimeOffset(UtcDateTimeConverter.ToUtc(revokedAt)).ToUnixTimeSeconds();
        var issuedAtSeconds = principal.GetIssuedAtUnixSeconds() ?? 0;
        var tokenId = principal.GetTokenId();
        var isExemptToken = tokenId is not null && string.Equals(tokenId, state.RevokeExceptJti, StringComparison.Ordinal);

        if (issuedAtSeconds <= revokedAtSeconds && !isExemptToken)
            context.Fail("Token has been revoked.");
    }

    private static async Task WriteUnauthorizedAsync(JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        if (context.Response.HasStarted)
            return;

        context.Response.Headers.WWWAuthenticate = JwtBearerDefaults.AuthenticationScheme;
        await ApiErrorResponse.WriteAsync(
            context.HttpContext,
            StatusCodes.Status401Unauthorized,
            ErrorCodes.Unauthorized,
            ErrorMessages.Unauthorized);
    }
}
