using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.VerifyEmailOtp
{
    public class VerifyEmailOtpCommandHandler : IRequestHandler<VerifyEmailOtpCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public VerifyEmailOtpCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> Handle(VerifyEmailOtpCommand request, CancellationToken cancellationToken)
        {
            var user = (await _userManager.FindByEmailAsync(request.Email))!;

            user.EmailConfirmed = true;
            user.Activate(_currentUserService.UserId.ToStringGuid());
            await _userManager.UpdateAsync(user);

            return Result<string>.Success(user.Email ?? string.Empty, LocalizationKeys.Auth.OtpVerified);
        }
    }
}
