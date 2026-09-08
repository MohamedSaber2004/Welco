using System.Security.Claims;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Product.Services.API.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            if (httpContext == null) return false;

            var env = httpContext.RequestServices.GetService<IWebHostEnvironment>();

            // In Development / Local / Test environments, permit access without strict JWT login
            if (env != null && (env.IsDevelopment() || env.EnvironmentName == "Test" || env.EnvironmentName == "Local"))
            {
                return true;
            }

            // In production, require authenticated user with Admin role
            var user = httpContext.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var userTypeClaim = user.FindFirst("userType")?.Value ?? user.FindFirst("UserType")?.Value;

            return user.IsInRole("Admin") ||
                   userRoles.Contains("Admin") ||
                   string.Equals(userTypeClaim, "Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
