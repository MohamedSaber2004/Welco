using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
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
        }
    }
}
