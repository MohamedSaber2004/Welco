using MediatR;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<Result<AuthResponseDto>>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
