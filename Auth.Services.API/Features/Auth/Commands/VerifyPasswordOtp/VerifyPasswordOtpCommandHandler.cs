using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Options;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.VerifyPasswordOtp
{
    public class VerifyPasswordOtpCommandHandler : IRequestHandler<VerifyPasswordOtpCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailSettings _emailSettings;

        public VerifyPasswordOtpCommandHandler(
            UserManager<ApplicationUser> userManager,
            IOptions<EmailSettings> emailSettings)
        {
            _userManager = userManager;
            _emailSettings = emailSettings.Value;
        }

        public async Task<Result<string>> Handle(VerifyPasswordOtpCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email?.Trim();
            var otpCode = request.OtpCode?.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otpCode))
            {
                return Result<string>.BadRequest(
                    LocalizationKeys.Auth.InvalidOtp,
                    new List<string> { LocalizationKeys.Auth.InvalidOtp });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<string>.NotFound(
                    LocalizationKeys.Auth.UserNotFound,
                    new List<string> { LocalizationKeys.Auth.UserNotFound });
            }

            if (!user.ValidatePasswordResetToken(otpCode))
            {
                return Result<string>.BadRequest(
                    LocalizationKeys.Auth.InvalidOtp,
                    new List<string> { LocalizationKeys.Auth.InvalidOtp });
            }

            return Result<string>.Success(otpCode, LocalizationKeys.Auth.OtpVerified);
        }
    }
}
