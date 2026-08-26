using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator(UserManager<ApplicationUser> userManager)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.EmailRequired)
                .EmailAddress().WithMessage(LocalizationKeys.Auth.EmailInvalid);

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.TokenRequired)
                .Length(6).WithMessage(LocalizationKeys.Auth.OtpCodeFormat);

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.NewPasswordRequired)
                .MinimumLength(6).WithMessage(LocalizationKeys.Auth.PasswordTooShort);

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.ConfirmPasswordRequired)
                .Equal(x => x.NewPassword).WithMessage(LocalizationKeys.Auth.PasswordMismatch);

            RuleFor(x => x).CustomAsync(async (command, context, ct) =>
            {
                if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Token))
                    return;

                var user = await userManager.FindByEmailAsync(command.Email);
                if (user == null)
                {
                    Result<string>.NotFound(LocalizationKeys.Auth.UserNotFound, 
                        new List<string> { LocalizationKeys.Auth.UserNotFound });
                    return;
                }

                if (!user.ValidatePasswordResetToken(command.Token))
                {
                    context.AddFailure(nameof(command.Token), LocalizationKeys.Auth.InvalidCredentials);
                }
            });
        }
    }
}
