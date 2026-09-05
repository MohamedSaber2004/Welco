using MediatR;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.Logout
{
    public class LogoutCommand : IRequest<Result<string>>
    {
        public string? RefreshToken { get; set; }

        /// <summary>
        /// When true, revoke every active refresh token of the user (all devices).
        /// Defaults to false so logout only ends the current device session.
        /// </summary>
        public bool RevokeAllSessions { get; set; }
    }
}
