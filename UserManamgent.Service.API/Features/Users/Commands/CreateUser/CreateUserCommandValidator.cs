using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(LocalizationKeys.UserManagement.FullNameRequired)
                .MaximumLength(150);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(LocalizationKeys.UserManagement.EmailRequired)
                .EmailAddress().WithMessage(LocalizationKeys.UserManagement.EmailInvalid)
                .MaximumLength(150);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(LocalizationKeys.UserManagement.PasswordRequired)
                .MinimumLength(6).WithMessage(LocalizationKeys.Auth.PasswordTooShort);

            RuleFor(x => x.UserType)
                .IsInEnum().WithMessage(LocalizationKeys.UserManagement.UserTypeRequired);
        }
    }
}
