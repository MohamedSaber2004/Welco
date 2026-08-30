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
        private const string DefaultFallbackSecret = "V5B?*77+gzD_pk+2!%ORg<i)<D$DH+Xf.nECc?];2l;";

        public static IServiceCollection AddWelcoJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings();
            configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

            var secret = !string.IsNullOrWhiteSpace(jwtSettings.Secret) && jwtSettings.Secret.Length >= 32
                ? jwtSettings.Secret
                : DefaultFallbackSecret;

            var validIssuers = jwtSettings.GetAllValidIssuers().ToList();
            var validAudiences = jwtSettings.GetAllValidAudiences().ToList();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
