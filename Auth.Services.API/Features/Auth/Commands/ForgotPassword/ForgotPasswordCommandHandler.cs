using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<string>.NotFound(
                    LocalizationKeys.Auth.UserNotFound,
                    new List<string> { LocalizationKeys.Auth.UserNotFound });
            }

            // Generate 6-digit OTP and store with 15-minute expiry
            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            user.RequestPasswordReset(otp, DateTime.UtcNow.AddMinutes(15));
            await _userManager.UpdateAsync(user);

            return Result<string>.Success(LocalizationKeys.Auth.OtpSent, LocalizationKeys.Auth.OtpSent);
        }
    }
}
