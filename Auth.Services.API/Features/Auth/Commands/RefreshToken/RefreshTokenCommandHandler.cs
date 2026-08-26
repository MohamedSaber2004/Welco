using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshRepo = _unitOfWork.GetRepository<UserRefreshToken, Guid>();
            var tokenEntity = await refreshRepo.GetFirstAsync(r => r.Token == request.RefreshToken && !r.IsRevoked, cancellationToken);

            if (tokenEntity == null)
            {
                return Result<AuthResponseDto>.BadRequest(
                    LocalizationKeys.Auth.InvalidRefreshToken,
                    new List<string> { LocalizationKeys.Auth.InvalidRefreshToken });
            }

            if (tokenEntity.ExpiryDate <= DateTime.UtcNow)
            {
                tokenEntity.Revoke();
                refreshRepo.Update(tokenEntity);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<AuthResponseDto>.BadRequest(
                    LocalizationKeys.Auth.RefreshTokenExpired,
                    new List<string> { LocalizationKeys.Auth.RefreshTokenExpired });
            }

            var user = await _userManager.FindByIdAsync(tokenEntity.UserId.ToString());
            if (user == null || user.IsDeleted || !user.IsActive)
            {
                return Result<AuthResponseDto>.BadRequest(
                    LocalizationKeys.Auth.UserNotFound,
                    new List<string> { LocalizationKeys.Auth.UserNotFound });
            }

            tokenEntity.Revoke();
            refreshRepo.Update(tokenEntity);

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles);
            var newRefreshTokenString = _jwtTokenService.GenerateRefreshToken(user);
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

            var newRefreshTokenEntity = UserRefreshToken.Create(user.Id, newRefreshTokenString, newRefreshTokenExpiry);
            await refreshRepo.AddAsync(newRefreshTokenEntity, cancellationToken);
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
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString,
                RefreshTokenExpiryTime = newRefreshTokenExpiry
            };

            return Result<AuthResponseDto>.Success(authResponse, LocalizationKeys.Auth.TokenRefreshed);
        }
    }
}
