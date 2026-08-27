using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.VerifyEmailOtp
{
    public class VerifyEmailOtpCommandValidator : AbstractValidator<VerifyEmailOtpCommand>
    {
        public VerifyEmailOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.EmailRequired)
                .EmailAddress().WithMessage(LocalizationKeys.Auth.EmailInvalid);

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.OtpCodeRequired)
                .Length(6).WithMessage(LocalizationKeys.Auth.OtpCodeFormat);
        }
    }
}
