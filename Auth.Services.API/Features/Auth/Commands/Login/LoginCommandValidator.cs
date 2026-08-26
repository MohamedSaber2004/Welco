using FluentValidation;
using Welco.Shared.Localization;

namespace Auth.Services.API.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.EmailRequired)
                .EmailAddress().WithMessage(LocalizationKeys.Auth.EmailInvalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.PasswordRequired);
        }
    }
}
