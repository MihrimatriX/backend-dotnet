using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EcommerceBackend.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtOptions _options;

        public JwtService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// HS256 token: <c>sub, email, role, jti, iat, exp, iss, aud</c> (§2, Spring ile aynı claim seti).
        /// </summary>
        public string GenerateToken(string email, long userId, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;

            var claims = new[]
            {
                new Claim(AuthClaimTypes.Subject, userId.ToString(CultureInfo.InvariantCulture)),
                new Claim(AuthClaimTypes.Email, email),
                new Claim(AuthClaimTypes.Role, role),
                new Claim(AuthClaimTypes.TokenId, Guid.NewGuid().ToString("N")),
                new Claim(
                    AuthClaimTypes.IssuedAt,
                    EpochTime.GetIntDate(now).ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer64),
            };

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: now.AddMinutes(_options.ExpirationInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
