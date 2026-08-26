using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Welco.Shared.Common.Interfaces;

namespace Welco.Shared.Common.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public Guid UserId { get; }

        public string? Email { get; }

        public bool IsAuthenticated { get; }

        public string? IpAddress { get; }

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var user = httpContext?.User;

            var userIdString = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user?.FindFirst("sub")?.Value
                ?? user?.FindFirst("nameid")?.Value;

            if (userIdString is not null && Guid.TryParse(userIdString, out var userId))
            {
                UserId = userId;
            }
            else
            {
                UserId = Guid.Empty;
            }

            Email = user?.FindFirst(ClaimTypes.Email)?.Value
                ?? user?.FindFirst("email")?.Value;

            IsAuthenticated = user?.Identity?.IsAuthenticated ?? false;
            IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
        }
    }
}
