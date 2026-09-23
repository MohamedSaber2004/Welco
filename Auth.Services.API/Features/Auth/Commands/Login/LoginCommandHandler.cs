using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
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

            if (user.UserType == UserType.OrganizationUser)
            {
                if (user.CompanyId.HasValue)
                {
                    var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
                    var company = await companyRepo.GetByIdAsync(user.CompanyId.Value, cancellationToken);
                    if (company == null || company.IsDeleted || company.Status != CompanyStatus.Approved)
                    {
                        return Result<AuthResponseDto>.Unauthorized(
                            LocalizationKeys.DistributorApplication.CompanyNotApproved,
                            new List<string> { LocalizationKeys.DistributorApplication.PendingApproval });
                    }
                }
                else
                {
                    var distRepo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
                    var userEmail = (user.Email ?? "").Trim().ToLower();
                    var hasApproved = await distRepo.ExistsAsync(
                        d => !d.IsDeleted && (d.ContactEmail.ToLower() == userEmail || d.CreatedBy.ToLower() == userEmail) && d.Status == DistributorApplicationStatus.Approved,
                        cancellationToken);
                    if (!hasApproved)
                    {
                        var hasPending = await distRepo.ExistsAsync(
                            d => !d.IsDeleted && (d.ContactEmail.ToLower() == userEmail || d.CreatedBy.ToLower() == userEmail) && d.Status == DistributorApplicationStatus.Pending,
                            cancellationToken);
                        var key = hasPending ? LocalizationKeys.DistributorApplication.PendingApproval : LocalizationKeys.DistributorApplication.NotApplied;
                        return Result<AuthResponseDto>.Unauthorized(key, new List<string> { key });
                    }

                    // Auto-heal company link if missing
                    var approvedApp = await distRepo.GetAll(d => !d.IsDeleted && (d.ContactEmail.ToLower() == userEmail || d.CreatedBy.ToLower() == userEmail) && d.Status == DistributorApplicationStatus.Approved)
                        .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (approvedApp != null)
                    {
                        var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
                        var comp = await companyRepo.GetAll(c => !c.IsDeleted && c.Name.ToLower() == approvedApp.CompanyName.ToLower() && c.Status == CompanyStatus.Approved)
                            .FirstOrDefaultAsync(cancellationToken);
                        if (comp != null)
                        {
                            user.CompanyId = comp.Id;
                            await _userManager.UpdateAsync(user);
                        }
                    }
                }
            }
            else if (user.UserType == UserType.Client)
            {
                var distRepo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
                var userEmail = (user.Email ?? "").Trim().ToLower();
                var approvedApp = await distRepo.GetAll(d => !d.IsDeleted && (d.ContactEmail.ToLower() == userEmail || d.CreatedBy.ToLower() == userEmail) && d.Status == DistributorApplicationStatus.Approved)
                    .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);
                if (approvedApp != null)
                {
                    user.UserType = UserType.OrganizationUser;
                    var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
                    var comp = await companyRepo.GetAll(c => !c.IsDeleted && c.Name.ToLower() == approvedApp.CompanyName.ToLower() && c.Status == CompanyStatus.Approved)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (comp != null)
                    {
                        user.CompanyId = comp.Id;
                    }
                    await _userManager.UpdateAsync(user);
                    if (!await _userManager.IsInRoleAsync(user, nameof(UserType.OrganizationUser)))
                    {
                        await _userManager.AddToRoleAsync(user, nameof(UserType.OrganizationUser));
                    }
                }
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
                CompanyId = user.CompanyId,
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
