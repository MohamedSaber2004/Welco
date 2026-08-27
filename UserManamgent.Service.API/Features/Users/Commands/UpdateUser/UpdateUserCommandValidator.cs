using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.UserManagement.UserIdRequired);

            RuleFor(x => x.FullName)
                .MaximumLength(150)
                .When(x => !string.IsNullOrEmpty(x.FullName));

            RuleFor(x => x.UserType)
                .IsInEnum().WithMessage(LocalizationKeys.UserManagement.UserTypeRequired)
                .When(x => x.UserType.HasValue);
        }
    }
}
