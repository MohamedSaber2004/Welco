using MediatR;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.ResendRegisterOtp
{
    public class ResendRegisterOtpCommand : IRequest<Result<string>>
    {
        public string Email { get; set; } = null!;
    }
}
