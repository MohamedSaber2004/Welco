using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;

namespace Auth.Services.API.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(LocalizationKeys.Auth.TokenRequired);

            RuleFor(x => x).CustomAsync(async (command, context, ct) =>
            {
                if (string.IsNullOrWhiteSpace(command.RefreshToken))
                    return;

                var refreshRepo = unitOfWork.GetRepository<UserRefreshToken, Guid>();
                var tokenEntity = await refreshRepo.GetFirstAsync(r => r.Token == command.RefreshToken, ct);

                if (tokenEntity == null || tokenEntity.IsRevoked)
                {
                    context.AddFailure(nameof(command.RefreshToken), LocalizationKeys.Auth.InvalidRefreshToken);
                    return;
                }

                if (tokenEntity.ExpiryDate <= DateTime.UtcNow)
                {
                    context.AddFailure(nameof(command.RefreshToken), LocalizationKeys.Auth.RefreshTokenExpired);
                    return;
                }

                var user = await userManager.FindByIdAsync(tokenEntity.UserId.ToString());
                if (user == null || user.IsDeleted || !user.IsActive)
                {
                    context.AddFailure(nameof(command.RefreshToken), LocalizationKeys.Auth.UserNotFound);
                }
            });
        }
    }
}
