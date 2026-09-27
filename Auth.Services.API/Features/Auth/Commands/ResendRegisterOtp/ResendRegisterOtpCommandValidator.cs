using FluentValidation;
using Welco.Shared.Localization;

namespace Auth.Services.API.Features.Auth.Commands.ResendRegisterOtp
{
    public class ResendRegisterOtpCommandValidator : AbstractValidator<ResendRegisterOtpCommand>
    {
        public ResendRegisterOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.EmailRequired)
                .EmailAddress().WithMessage(LocalizationKeys.Auth.EmailInvalid);
        }
    }
}
