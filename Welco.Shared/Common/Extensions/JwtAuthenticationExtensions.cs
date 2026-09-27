using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Welco.Shared.Common.Options;
using Welco.Shared.Localization;
using Welco.Shared.Localization.Interfaces;

namespace Welco.Shared.Common.Extensions
{
    public static class JwtAuthenticationExtensions
    {
        private const string DevelopmentFallbackSecret = "V5B?*77+gzD_pk+2!%ORg<i)<D$DH+Xf.nECc?];2l;";

        private const int MinimumSecretLength = 32;

        public static IServiceCollection AddWelcoJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings();
            configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

            var configuredSecret = jwtSettings.Secret;
            var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? configuration["DOTNET_ENVIRONMENT"] ?? string.Empty;
            var isDevelopment =
                environment.Equals("Development", StringComparison.OrdinalIgnoreCase) ||
                environment.Equals("Test", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(configuredSecret) || configuredSecret.Length < MinimumSecretLength)
            {
                if (!isDevelopment)
                {
                    throw new InvalidOperationException(
                        $"'{JwtSettings.SectionName}:{nameof(JwtSettings.Secret)}' is missing or shorter than " +
                        $"{MinimumSecretLength} characters. It MUST match the secret used by the token issuer " +
                        "(Auth service) or every authenticated request will fail validation. Set it via " +
                        "configuration (appsettings) or the JwtSettings__Secret environment variable.");
                }

                configuredSecret = DevelopmentFallbackSecret;
            }

            var secret = configuredSecret;

            var validIssuers = jwtSettings.GetAllValidIssuers().ToList();
            var validAudiences = jwtSettings.GetAllValidAudiences().ToList();

            // The default scheme must be pinned with the options overload, not the
            // `AddAuthentication(string)` one. `AddIdentity<,>()` runs earlier in most
            // services and sets DefaultAuthenticateScheme/DefaultChallengeScheme to
            // "Identity.Application"; `AddAuthentication("Bearer")` only sets
            // DefaultScheme, which ASP.NET Core consults *after* the authenticate
            // scheme. The result was that UseAuthentication() ran cookie auth, left
            // HttpContext.User anonymous, and every handler reading ICurrentUserService
            // saw Guid.Empty — except those behind RoleAuthorizeAttribute, which
            // re-authenticates explicitly and masked the fault.
            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                        ValidateIssuer = validIssuers.Count > 0,
                        ValidIssuers = validIssuers.Count > 0 ? validIssuers : null,
                        ValidateAudience = validAudiences.Count > 0,
                        ValidAudiences = validAudiences.Count > 0 ? validAudiences : null,
                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async ctx =>
                        {
                            ctx.HandleResponse();
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            ctx.Response.ContentType = "application/json";
                            var loc = ctx.HttpContext.RequestServices.GetService<ILocalizationProvider>();
                            var lang = ctx.Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0].Trim().ToLowerInvariant().StartsWith("ar") == true ? "ar" : "en";
                            var msg = loc?.GetLocalizedString("ExceptionMessages.Unauthorized", lang) ?? "Unauthorized";
                            await ctx.Response.WriteAsJsonAsync(new
                            {
                                isSuccess = false,
                                statusCode = 401,
                                message = msg,
                                errors = new[] { msg },
                                data = (object?)null
                            });
                        }
                    };
                });

            return services;
        }
    }
}
