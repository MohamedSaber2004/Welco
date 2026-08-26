using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.VerifyEmailOtp
{
    public class VerifyEmailOtpCommandHandler : IRequestHandler<VerifyEmailOtpCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyEmailOtpCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<string>> Handle(VerifyEmailOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<string>.NotFound(
                    LocalizationKeys.Auth.UserNotFound,
                    new List<string> { LocalizationKeys.Auth.UserNotFound });
            }

            user.EmailConfirmed = true;
            user.Activate(user.Email ?? "System");
            await _userManager.UpdateAsync(user);

            return Result<string>.Success(user.Email ?? string.Empty, LocalizationKeys.Auth.OtpVerified);
        }
    }
}
