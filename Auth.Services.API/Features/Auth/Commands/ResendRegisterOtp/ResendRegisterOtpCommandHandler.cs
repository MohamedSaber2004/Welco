using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.ResendRegisterOtp
{
    /// <summary>
    /// Issues a fresh registration verification code for an account that has not
    /// confirmed its email yet. This replaces the client-side hack of replaying
    /// the whole <c>register</c> call, which could only ever fail with
    /// "email already registered" once the account existed.
    /// </summary>
    public class ResendRegisterOtpCommandHandler
        : IRequestHandler<ResendRegisterOtpCommand, Result<string>>
    {
        // Minimum gap between two sends for the same address, so the endpoint
        // cannot be used to mail-bomb an inbox.
        private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(45);

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public ResendRegisterOtpCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IOptions<EmailSettings> emailSettings)
        {
            _userManager = userManager;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
        }

        public async Task<Result<string>> Handle(ResendRegisterOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<string>.NotFound(
                    LocalizationKeys.Auth.UserNotFound,
                    new List<string> { LocalizationKeys.Auth.UserNotFound });
            }

            // Already verified (or an org user, who is pre-confirmed): tell the
            // caller to sign in instead of silently re-sending a code.
            if (user.EmailConfirmed)
            {
                return Result<string>.BadRequest(
                    LocalizationKeys.Auth.EmailAlreadyConfirmed,
                    new List<string> { LocalizationKeys.Auth.EmailAlreadyConfirmed });
            }

            // Honour the cooldown, but let a user whose code already expired resend.
            var stillValid =
                user.EmailConfirmationOtpExpiry.HasValue &&
                user.EmailConfirmationOtpExpiry.Value > DateTime.UtcNow;

            if (stillValid && user.UpdatedAt.HasValue &&
                DateTime.UtcNow - user.UpdatedAt.Value < ResendCooldown)
            {
                return Result<string>.BadRequest(
                    LocalizationKeys.Auth.OtpResendTooSoon,
                    new List<string> { LocalizationKeys.Auth.OtpResendTooSoon });
            }

            var expiryMinutes =
                _emailSettings.VerificationCodeExpiryMinutes > 0
                    ? _emailSettings.VerificationCodeExpiryMinutes
                    : 10;

            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            user.SetEmailConfirmationOtp(otp, DateTime.UtcNow.AddMinutes(expiryMinutes));
            user.MarkAsUpdated(user.Id.ToString());

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToList();
                return Result<string>.BadRequest(
                    errors.FirstOrDefault() ?? LocalizationKeys.ExceptionMessages.BadRequest,
                    errors);
            }

            await _emailService.SendVerificationEmailAsync(
                user.Email!,
                otp,
                user.Language.ToString().ToLower(),
                cancellationToken);

            return Result<string>.Success(user.Email!, LocalizationKeys.Auth.OtpResent);
        }
    }
}
