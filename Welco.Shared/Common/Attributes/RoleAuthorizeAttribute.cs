using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Localization.Interfaces;
using Welco.Shared.Results;

namespace Welco.Shared.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RoleAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeAttribute()
        {
            _allowedRoles = Array.Empty<string>();
        }

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles ?? Array.Empty<string>();
        }

        public RoleAuthorizeAttribute(params UserType[] userTypes)
        {
            _allowedRoles = userTypes?.Select(u => u.ToString()).ToArray() ?? Array.Empty<string>();
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em is IAllowAnonymous);

            if (hasAllowAnonymous)
            {
                return;
            }

            if (context.HttpContext.User?.Identity == null || !context.HttpContext.User.Identity.IsAuthenticated)
            {
                var authenticateResult = await context.HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                if (authenticateResult.Succeeded)
                {
                    context.HttpContext.User = authenticateResult.Principal;
                }
            }

            var user = context.HttpContext.User;
            var localizationProvider = context.HttpContext.RequestServices.GetService<ILocalizationProvider>();
            var culture = GetRequestCulture(context.HttpContext);

            string Localize(string key)
            {
                if (localizationProvider == null || string.IsNullOrWhiteSpace(key))
                    return key;

                return localizationProvider.GetLocalizedString(key, culture);
            }

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                var message = Localize(LocalizationKeys.ExceptionMessages.Unauthorized);
                var response = Result<object?>.Unauthorized(message, new List<string> { message });

                context.Result = new JsonResult(response)
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var allowedRoles = _allowedRoles;

            if (allowedRoles.Length == 0)
            {
                return;
            }

            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var userTypeClaim = user.FindFirst("userType")?.Value ?? user.FindFirst("UserType")?.Value;

            if (user.IsInRole("Admin") ||
                userRoles.Contains("Admin") ||
                (!string.IsNullOrWhiteSpace(userTypeClaim) && string.Equals(userTypeClaim, UserType.Admin.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            var isAuthorized = allowedRoles.Any(role =>
                user.IsInRole(role) ||
                userRoles.Contains(role) ||
                (!string.IsNullOrWhiteSpace(userTypeClaim) && string.Equals(userTypeClaim, role, StringComparison.OrdinalIgnoreCase)));

            if (!isAuthorized)
            {
                var message = Localize(LocalizationKeys.ExceptionMessages.Forbidden);
                var response = Result<object?>.Forbidden(message, new List<string> { message });

                context.Result = new JsonResult(response)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }

        private static string GetRequestCulture(HttpContext context)
        {
            var req = context?.Request;
            if (req != null)
            {
                var headers = req.Headers;
                var hCulture = headers["Accept-Language"].FirstOrDefault()
                               ?? headers["Language"].FirstOrDefault()
                               ?? headers["language"].FirstOrDefault()
                               ?? headers["Culture"].FirstOrDefault()
                               ?? headers["Lang"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(hCulture))
                {
                    return AppLanguageExtensions.FromCode(hCulture).ToCode();
                }

                var qCulture = req.Query["culture"].FirstOrDefault()
                               ?? req.Query["lang"].FirstOrDefault()
                               ?? req.Query["language"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(qCulture))
                {
                    return AppLanguageExtensions.FromCode(qCulture).ToCode();
                }
            }

            var current = System.Globalization.CultureInfo.CurrentUICulture?.Name;
            return AppLanguageExtensions.FromCode(current).ToCode();
        }
    }
}
