using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator(UserManager<ApplicationUser> userManager)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.EmailRequired)
                .EmailAddress().WithMessage(LocalizationKeys.Auth.EmailInvalid);

            RuleFor(x => x.Email).CustomAsync(async (email, context, ct) =>
            {
                if (string.IsNullOrWhiteSpace(email))
                    return;

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    Result<string>.NotFound(LocalizationKeys.Auth.UserNotFound,
                        new List<string> { LocalizationKeys.Auth.UserNotFound });
                }
            });
        }
    }
}
