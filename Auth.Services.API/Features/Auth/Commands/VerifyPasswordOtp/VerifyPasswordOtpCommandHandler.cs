using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.VerifyPasswordOtp
{
    public class VerifyPasswordOtpCommandHandler : IRequestHandler<VerifyPasswordOtpCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyPasswordOtpCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<string>> Handle(VerifyPasswordOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<string>.NotFound(
                    LocalizationKeys.Auth.UserNotFound,
                    new List<string> { LocalizationKeys.Auth.UserNotFound });
            }

            if (!user.ValidatePasswordResetToken(request.OtpCode))
            {
                return Result<string>.BadRequest(
                    LocalizationKeys.Auth.InvalidOtp,
                    new List<string> { LocalizationKeys.Auth.InvalidOtp });
            }

            // Generate 6-digit reset token and store in user model (valid for 15 minutes)
            var resetToken = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            user.RequestPasswordReset(resetToken, DateTime.UtcNow.AddMinutes(15));
            await _userManager.UpdateAsync(user);

            return Result<string>.Success(resetToken, LocalizationKeys.Auth.OtpVerified);
        }
    }
}
