using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
                UserType = request.UserType,
                Language = request.Language,
                IsActive = true,
                EmailConfirmed = false
            };

            await _userManager.CreateAsync(user, request.Password);
            await _userManager.AddToRoleAsync(user, request.UserType.ToString());

            // Generate 6-digit Email Confirmation OTP (valid for 15 minutes)
            var emailOtp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            await _userManager.SetAuthenticationTokenAsync(user, "WelcoAuth", "EmailConfirmationOtp", emailOtp);
            await _userManager.SetAuthenticationTokenAsync(user, "WelcoAuth", "EmailConfirmationOtpExpiry", DateTime.UtcNow.AddMinutes(10).ToString("O"));

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
                Email = user.Email,
                UserName = user.UserName,
                UserType = user.UserType,
                Language = user.Language,
                Roles = roles,
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                RefreshTokenExpiryTime = refreshTokenExpiry
            };

            return Result<AuthResponseDto>.Success(authResponse, LocalizationKeys.Auth.RegisterSuccess);
        }
    }
}
