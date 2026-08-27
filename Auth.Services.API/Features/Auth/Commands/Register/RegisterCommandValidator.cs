using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator(UserManager<ApplicationUser> userManager)
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.FullNameRequired);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.EmailRequired)
                .EmailAddress().WithMessage(LocalizationKeys.Auth.EmailInvalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.PasswordRequired)
                .MinimumLength(6).WithMessage(LocalizationKeys.Auth.PasswordTooShort);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.ConfirmPasswordRequired)
                .Equal(x => x.Password).WithMessage(LocalizationKeys.Auth.PasswordMismatch);

            RuleFor(x => x.UserType)
                .IsInEnum();

            RuleFor(x => x.Language)
                .IsInEnum();

            RuleFor(x => x).CustomAsync(async (command, context, ct) =>
            {
                if (string.IsNullOrWhiteSpace(command.Email))
                    return;

                var existingUser = await userManager.FindByEmailAsync(command.Email);
                if (existingUser != null)
                {
                    Result<string>.BadRequest(LocalizationKeys.Auth.EmailAlreadyExists,
                        new List<string> { LocalizationKeys.Auth.EmailAlreadyExists });
                }
            });
        }
    }
}
