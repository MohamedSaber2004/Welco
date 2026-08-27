using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.VerifyEmailOtp
{
    public class VerifyEmailOtpCommandHandler : IRequestHandler<VerifyEmailOtpCommand, Result<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUnitOfWork _unitOfWork;

        public VerifyEmailOtpCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthResponseDto>> Handle(VerifyEmailOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<AuthResponseDto>.NotFound(
                    LocalizationKeys.Auth.UserNotFound,
                    new List<string> { LocalizationKeys.Auth.UserNotFound });
            }

            if (!user.ValidateEmailConfirmationOtp(request.OtpCode))
            {
                return Result<AuthResponseDto>.BadRequest(
                    LocalizationKeys.Auth.InvalidOtp,
                    new List<string> { LocalizationKeys.Auth.InvalidOtp });
            }

            user.EmailConfirmed = true;
            user.ClearEmailConfirmationOtp();
            user.Activate(user.Email ?? "System");
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
            var refreshTokenString = _jwtTokenService.GenerateRefreshToken(user);
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(30);

            var refreshTokenEntity = UserRefreshToken.Create(user.Id, refreshTokenString, refreshTokenExpiry);
            var refreshRepo = _unitOfWork.GetRepository<UserRefreshToken, Guid>();
            await refreshRepo.AddAsync(refreshTokenEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var authResponse = new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName,
                UserType = user.UserType,
                Language = user.Language,
                Roles = roles,
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                RefreshTokenExpiryTime = refreshTokenExpiry
            };

            return Result<AuthResponseDto>.Success(authResponse, LocalizationKeys.Auth.OtpVerified);
        }
    }
}
