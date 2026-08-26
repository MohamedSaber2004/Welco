using MediatR;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<Result<AuthResponseDto>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
