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

namespace Auth.Services.API.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProfileCommandHandler(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
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

            var currentUserId = userId.ToString();

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.FullName = request.FullName.Trim();
            }

            if (request.PhoneNumber != null)
            {
                user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            }

            if (request.ProfilePictureName != null)
            {
                user.ProfilePictureName = string.IsNullOrWhiteSpace(request.ProfilePictureName) ? null : request.ProfilePictureName.Trim();
            }

            if (request.Language.HasValue)
            {
                user.Language = request.Language.Value;
            }

            user.MarkAsUpdated(currentUserId);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToList();
                return Result<UserProfileDto>.BadRequest(
                    errors.FirstOrDefault() ?? LocalizationKeys.ExceptionMessages.BadRequest,
                    errors);
            }

            var addressRepo = _unitOfWork.GetRepository<UserAddress, Guid>();

            if (request.Addresses != null)
            {
                var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                var cityRepo = _unitOfWork.GetRepository<City, Guid>();
                var zoneRepo = _unitOfWork.GetRepository<Zone, Guid>();

                var existingAddresses = await addressRepo
                    .GetAll(a => a.UserId == userId && !a.IsDeleted)
                    .ToListAsync(cancellationToken);

                var incomingAddressIds = request.Addresses
                    .Where(a => a.Id.HasValue && a.Id.Value != Guid.Empty)
                    .Select(a => a.Id!.Value)
                    .ToHashSet();

                foreach (var incomingId in incomingAddressIds)
                {
                    if (!existingAddresses.Any(a => a.Id == incomingId))
                    {
                        return Result<UserProfileDto>.NotFound(
                            LocalizationKeys.UserAddress.AddressNotFound,
                            new List<string> { LocalizationKeys.UserAddress.AddressNotFound });
                    }
                }

                // Validate country, city, zone references for every incoming address
                foreach (var addrDto in request.Addresses)
                {
                    var country = await countryRepo.GetByIdAsync(addrDto.CountryId, cancellationToken);
                    if (country == null || country.IsDeleted)
                    {
                        return Result<UserProfileDto>.NotFound(
                            LocalizationKeys.Country.NotFound,
                            new List<string> { LocalizationKeys.Country.NotFound });
                    }

                    var city = await cityRepo.GetByIdAsync(addrDto.CityId, cancellationToken);
                    if (city == null || city.IsDeleted || city.CountryId != addrDto.CountryId)
                    {
                        return Result<UserProfileDto>.NotFound(
                            LocalizationKeys.City.NotFound,
                            new List<string> { LocalizationKeys.City.NotFound });
                    }

                    var zone = await zoneRepo.GetByIdAsync(addrDto.ZoneId, cancellationToken);
                    if (zone == null || zone.IsDeleted || zone.CityId != addrDto.CityId)
                    {
                        return Result<UserProfileDto>.NotFound(
                            LocalizationKeys.Zone.NotFound,
                            new List<string> { LocalizationKeys.Zone.NotFound });
                    }
                }

                // Soft-delete addresses that are not present in incoming list
                foreach (var existing in existingAddresses)
                {
                    if (!incomingAddressIds.Contains(existing.Id))
                    {
                        existing.MarkAsDeleted(currentUserId);
                        addressRepo.Update(existing);
                    }
                }

                // Update existing or create new addresses
                foreach (var addrDto in request.Addresses)
                {
                    if (addrDto.Id.HasValue && addrDto.Id.Value != Guid.Empty)
                    {
                        var existing = existingAddresses.First(a => a.Id == addrDto.Id.Value);
                        existing.Update(
                            addrDto.CountryId,
                            addrDto.CityId,
                            addrDto.ZoneId,
                            addrDto.Street,
                            addrDto.Building,
                            addrDto.Floor,
                            addrDto.Apartment,
                            currentUserId);
                        addressRepo.Update(existing);
                    }
                    else
                    {
                        var newAddress = UserAddress.Create(
                            userId,
                            addrDto.CountryId,
                            addrDto.CityId,
                            addrDto.ZoneId,
                            addrDto.Street,
                            addrDto.Building,
                            addrDto.Floor,
                            addrDto.Apartment,
                            currentUserId);
                        await addressRepo.AddAsync(newAddress, cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var addresses = await addressRepo
                .GetAll(a => a.UserId == userId && !a.IsDeleted)
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

            var roles = await _userManager.GetRolesAsync(user);

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

            return Result<UserProfileDto>.Success(profile, LocalizationKeys.Auth.ProfileUpdated);
        }
    }
}
