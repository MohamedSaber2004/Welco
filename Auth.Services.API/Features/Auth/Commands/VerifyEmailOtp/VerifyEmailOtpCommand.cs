using MediatR;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.VerifyEmailOtp
{
    public class VerifyEmailOtpCommand : IRequest<Result<AuthResponseDto>>
    {
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
    }
}
