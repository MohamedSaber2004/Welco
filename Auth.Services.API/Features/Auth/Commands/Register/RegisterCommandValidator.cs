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
                .IsInEnum().WithMessage(LocalizationKeys.Auth.UserTypeRequired);

            RuleFor(x => x.Language)
                .IsInEnum().WithMessage(LocalizationKeys.Auth.LanguageRequired);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number is too long")
                .Matches(@"^\+?[0-9\s\-]{6,20}$").When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Phone number must be 6-20 digits, may start with +");

            RuleFor(x => x.PhoneCountryId)
                .Must(id => id == null || id != Guid.Empty).WithMessage(LocalizationKeys.Country.NotFound);

            RuleFor(x => x).CustomAsync(async (command, context, ct) =>
            {
                if (string.IsNullOrWhiteSpace(command.Email))
                    return;

                var existingUser = await userManager.FindByEmailAsync(command.Email);
                if (existingUser != null)
                {
                    context.AddFailure(nameof(command.Email), LocalizationKeys.Auth.EmailAlreadyExists);
                }
            });
        }
    }
}
