using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Welco.Shared.Common.Options;

namespace Welco.Shared.Common.Attributes
{
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ServiceAuthAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var settings = context.HttpContext.RequestServices
                .GetRequiredService<IOptions<WelcoServiceSettings>>().Value;

            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<ServiceAuthAttribute>();

            var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            var token = authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
                ? authHeader["Bearer ".Length..].Trim()
                : null;

            if (string.IsNullOrWhiteSpace(token))
            {
                logger.LogWarning("[ServiceAuth] Missing token for Path={Path}", context.HttpContext.Request.Path);
                context.Result = new UnauthorizedObjectResult(new
                {
                    isSuccess = false,
                    statusCode = 401,
                    message = "Service authentication required.",
                    errors = new[] { "Missing service token." },
                    data = (object?)null
                });
                return;
            }

            if (!ValidateServiceToken(token, settings, logger))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    isSuccess = false,
                    statusCode = 401,
                    message = "Service authentication failed.",
                    errors = new[] { "Invalid or expired service token." },
                    data = (object?)null
                });
                return;
            }

            logger.LogInformation("[ServiceAuth] Authenticated service request for Path={Path}", context.HttpContext.Request.Path);

            await next();
        }

        private static bool ValidateServiceToken(
            string token,
            WelcoServiceSettings settings,
            ILogger logger)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();

                JwtSecurityToken? jwt;
                try
                {
                    jwt = handler.ReadJwtToken(token);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "[ServiceAuth] Unable to read service token.");
                    return false;
                }

                var clientId = jwt.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;

                string secret;
                if (!string.IsNullOrWhiteSpace(clientId))
                {
                    logger.LogWarning("[ServiceAuth] Client-specific authentication is no longer supported.");
                    return false;
                }

                logger.LogWarning("[ServiceAuth] Legacy service token without client_id.");
                if (string.IsNullOrWhiteSpace(settings.ServiceSecret) || settings.ServiceSecret.Length < 32)
                {
                    logger.LogError("[ServiceAuth] WelcoServiceSettings.ServiceSecret is not configured (min 32 chars).");
                    return false;
                }

                secret = settings.ServiceSecret;

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = !string.IsNullOrWhiteSpace(settings.ServiceIssuer),
                    ValidIssuer = settings.ServiceIssuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(settings.ServiceAudience),
                    ValidAudience = settings.ServiceAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                }, out _);

                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                logger.LogWarning("[ServiceAuth] Expired service token.");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[ServiceAuth] Token validation failed.");
                return false;
            }
        }
    }
}
