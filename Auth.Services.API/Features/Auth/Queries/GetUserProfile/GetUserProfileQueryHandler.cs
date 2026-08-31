using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Queries.GetUserProfile
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetUserProfileQueryHandler> _logger;

        public GetUserProfileQueryHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ILogger<GetUserProfileQueryHandler> logger)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == Guid.Empty)
            {
                return Result<UserProfileDto>.Unauthorized(
                    LocalizationKeys.ExceptionMessages.Unauthorized,
                    new List<string> { LocalizationKeys.ExceptionMessages.Unauthorized });
            }

            var userId = _currentUserService.UserId;
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
            {
                return Result<UserProfileDto>.NotFound(
                    LocalizationKeys.Auth.UserNotFound,
                    new List<string> { LocalizationKeys.Auth.UserNotFound });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var addresses = new List<UserAddressDto>();
            try
            {
                var addressRepo = _unitOfWork.GetRepository<UserAddress, Guid>();
                addresses = await addressRepo
                    .GetAll(a => a.UserId == user.Id && !a.IsDeleted)
                    .Select(a => new UserAddressDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        CountryId = a.CountryId,
                        CountryNameEn = a.Country != null ? a.Country.NameEn : null,
                        CountryNameAr = a.Country != null ? a.Country.NameAr : null,
                        CityId = a.CityId,
                        CityNameEn = a.City != null ? a.City.NameEn : null,
                        CityNameAr = a.City != null ? a.City.NameAr : null,
                        ZoneId = a.ZoneId,
                        ZoneNameEn = a.Zone != null ? a.Zone.NameEn : null,
                        ZoneNameAr = a.Zone != null ? a.Zone.NameAr : null,
                        Street = a.Street,
                        Building = a.Building,
                        Floor = a.Floor,
                        Apartment = a.Apartment,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt
                    })
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load addresses for user {UserId}", user.Id);
            }

            var profile = new UserProfileDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureName = user.ProfilePictureName,
                UserType = user.UserType,
                CompanyId = user.CompanyId,
                Language = user.Language,
                IsEmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                Roles = roles,
                Addresses = addresses
            };

            return Result<UserProfileDto>.Success(profile, LocalizationKeys.Auth.ProfileFetched);
        }
    }
}
