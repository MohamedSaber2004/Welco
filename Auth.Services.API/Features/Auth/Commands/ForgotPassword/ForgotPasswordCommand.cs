using MediatR;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<Result<string>>
    {
        public string Email { get; set; } = string.Empty;
    }
}
