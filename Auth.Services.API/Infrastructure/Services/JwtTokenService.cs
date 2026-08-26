using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;

namespace Auth.Services.API.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;
        private const string DefaultFallbackSecret = "V5B?*77+gzD_pk+2!%ORg<i)<D$DH+Xf.nECc?];2l;";

        public JwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        private string SecretKey => !string.IsNullOrWhiteSpace(_settings.Secret) && _settings.Secret.Length >= 32
            ? _settings.Secret
            : DefaultFallbackSecret;

        public string GenerateAccessToken(ApplicationUser user, IList<string> roles, Guid? clinicId = null, bool hasActiveSubscription = false)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("FullName", user.FullName ?? string.Empty)
            };

            var userTypesMask = roles
                .Select(r => Enum.TryParse<UserType>(r, ignoreCase: true, out var ut) ? (int)ut : 0)
                .Aggregate(0, (acc, val) => acc | val);

            claims.Add(new Claim("UserTypes", userTypesMask.ToString()));

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(_settings.ExpiryInMinutes > 0 ? _settings.ExpiryInMinutes : 60);

            var token = new JwtSecurityToken(
                !string.IsNullOrWhiteSpace(_settings.Issuer) ? _settings.Issuer : null,
                !string.IsNullOrWhiteSpace(_settings.Audience) ? _settings.Audience : null,
                claims,
                expires: expiry,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("TokenType", "RefreshToken")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays > 0 ? _settings.RefreshTokenExpiryDays : 30);

            var token = new JwtSecurityToken(
                !string.IsNullOrWhiteSpace(_settings.Issuer) ? _settings.Issuer : null,
                !string.IsNullOrWhiteSpace(_settings.Audience) ? _settings.Audience : null,
                claims,
                expires: expiry,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = !string.IsNullOrWhiteSpace(_settings.Audience),
                ValidAudience = !string.IsNullOrWhiteSpace(_settings.Audience) ? _settings.Audience : null,
                ValidateIssuer = !string.IsNullOrWhiteSpace(_settings.Issuer),
                ValidIssuer = !string.IsNullOrWhiteSpace(_settings.Issuer) ? _settings.Issuer : null,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
