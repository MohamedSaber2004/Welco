using MediatR;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.VerifyPasswordOtp
{
    public class VerifyPasswordOtpCommand : IRequest<Result<string>>
    {
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
    }
}
