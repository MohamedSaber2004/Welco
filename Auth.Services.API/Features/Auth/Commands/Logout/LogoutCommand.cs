using MediatR;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.Logout
{
    public class LogoutCommand : IRequest<Result<string>>
    {
        public string? RefreshToken { get; set; }
    }
}
