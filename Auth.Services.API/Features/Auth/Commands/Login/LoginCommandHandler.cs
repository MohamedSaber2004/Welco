using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUnitOfWork _unitOfWork;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
                       ?? await _userManager.FindByNameAsync(request.Email);

            if (user == null)
            {
                return Result<AuthResponseDto>.Unauthorized(
                    LocalizationKeys.Auth.InvalidCredentials,
                    new List<string> { LocalizationKeys.Auth.InvalidCredentials });
            }

            if (user.IsDeleted || !user.IsActive)
            {
                return Result<AuthResponseDto>.Unauthorized(
                    LocalizationKeys.Auth.AccountDeactivated,
                    new List<string> { LocalizationKeys.Auth.AccountDeactivated });
            }

            if (!user.EmailConfirmed)
            {
                return Result<AuthResponseDto>.Unauthorized(
                    LocalizationKeys.Auth.EmailNotConfirmed,
                    new List<string> { LocalizationKeys.Auth.EmailNotConfirmed });
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return Result<AuthResponseDto>.Unauthorized(
                    LocalizationKeys.Auth.InvalidCredentials,
                    new List<string> { LocalizationKeys.Auth.InvalidCredentials });
            }

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

            return Result<AuthResponseDto>.Success(authResponse, LocalizationKeys.Auth.LoginSuccess);
        }
    }
}
