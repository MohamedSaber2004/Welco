using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Users.Commands.ChangeUserPassword
{
    public class ChangeUserPasswordCommandValidator : AbstractValidator<ChangeUserPasswordCommand>
    {
        public ChangeUserPasswordCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.UserManagement.UserIdRequired);

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(LocalizationKeys.UserManagement.PasswordRequired)
                .MinimumLength(6).WithMessage(LocalizationKeys.Auth.PasswordTooShort);
        }
    }
}
