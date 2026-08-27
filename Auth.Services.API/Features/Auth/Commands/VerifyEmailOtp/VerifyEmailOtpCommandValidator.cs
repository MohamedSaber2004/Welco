using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.VerifyEmailOtp
{
    public class VerifyEmailOtpCommandValidator : AbstractValidator<VerifyEmailOtpCommand>
    {
        public VerifyEmailOtpCommandValidator(UserManager<ApplicationUser> userManager)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.EmailRequired)
                .EmailAddress().WithMessage(LocalizationKeys.Auth.EmailInvalid);

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.OtpCodeRequired)
                .Length(6).WithMessage(LocalizationKeys.Auth.OtpCodeFormat);

            RuleFor(x => x).CustomAsync(async (command, context, ct) =>
            {
                if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.OtpCode))
                    return;

                var user = await userManager.FindByEmailAsync(command.Email);
                if (user == null)
                {
                    context.AddFailure(nameof(command.Email), LocalizationKeys.Auth.UserNotFound);
                    return;
                }

                if (!user.ValidateEmailConfirmationOtp(command.OtpCode))
                {
                    context.AddFailure(nameof(command.OtpCode), LocalizationKeys.Auth.InvalidOtp);
                }
            });
        }
    }
}
