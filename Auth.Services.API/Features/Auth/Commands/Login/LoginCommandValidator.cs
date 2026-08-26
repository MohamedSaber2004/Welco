using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;

namespace Auth.Services.API.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator(UserManager<ApplicationUser> userManager)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.EmailRequired)
                .EmailAddress().WithMessage(LocalizationKeys.Auth.EmailInvalid);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.PasswordRequired);

            RuleFor(x => x).CustomAsync(async (command, context, ct) =>
            {
                if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
                    return;

                var user = await userManager.FindByEmailAsync(command.Email)
                           ?? await userManager.FindByNameAsync(command.Email);

                if (user == null)
                {
                    context.AddFailure(nameof(command.Email), LocalizationKeys.Auth.InvalidCredentials);
                    return;
                }

                if (user.IsDeleted || !user.IsActive)
                {
                    context.AddFailure(nameof(command.Email), LocalizationKeys.Auth.AccountDeactivated);
                    return;
                }

                var isPasswordValid = await userManager.CheckPasswordAsync(user, command.Password);
                if (!isPasswordValid)
                {
                    context.AddFailure(nameof(command.Password), LocalizationKeys.Auth.InvalidCredentials);
                }
            });
        }
    }
}
