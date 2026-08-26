using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<string>.NotFound(
                    LocalizationKeys.Auth.UserNotFound,
                    new List<string> { LocalizationKeys.Auth.UserNotFound });
            }

            if (!user.ValidatePasswordResetToken(request.Token))
            {
                return Result<string>.BadRequest(
                    LocalizationKeys.Auth.InvalidCredentials,
                    new List<string> { LocalizationKeys.Auth.InvalidCredentials });
            }

            var identityResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, identityResetToken, request.NewPassword);

            if (!resetResult.Succeeded)
            {
                var errors = resetResult.Errors.Select(e => e.Description).ToList();
                return Result<string>.BadRequest(
                    errors.FirstOrDefault() ?? LocalizationKeys.Auth.InvalidCredentials,
                    errors);
            }

            user.ClearPasswordResetToken();
            await _userManager.UpdateAsync(user);

            return Result<string>.Success(LocalizationKeys.Auth.PasswordResetSuccess, LocalizationKeys.Auth.PasswordResetSuccess);
        }
    }
}
