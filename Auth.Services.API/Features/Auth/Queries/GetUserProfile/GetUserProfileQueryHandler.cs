using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
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
                    .GetAllWithIncluding(a => a.UserId == user.Id && !a.IsDeleted, a => a.Country, a => a.City, a => a.Zone)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .Select(a => new UserAddressDto
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        CountryId = a.CountryId,
                        CountryNameEn = a.Country != null ? a.Country.NameEn : null,
                        CountryNameAr = a.Country != null ? a.Country.NameAr : null,
                        CountryCode = a.Country != null ? a.Country.Code : null,
                        CountryPhoneCode = a.Country != null ? a.Country.PhoneCode : null,
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
                        IsDefault = a.IsDefault,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt
                    })
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load addresses for user {UserId}", user.Id);
            }

            string? phoneCode = null;
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                var cleanPhone = user.PhoneNumber.Trim().Replace(" ", "").Replace("-", "");
                try
                {
                    var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                    var allCountries = await countryRepo.GetAllListAsync(c => !c.IsDeleted && c.PhoneCode != null, cancellationToken);
                    var matched = allCountries
                        .Where(c => !string.IsNullOrWhiteSpace(c.PhoneCode) && cleanPhone.StartsWith(c.PhoneCode!.Trim().Replace(" ", ""), StringComparison.Ordinal))
                        .OrderByDescending(c => c.PhoneCode!.Length)
                        .FirstOrDefault();

                    if (matched != null)
                    {
                        phoneCode = matched.PhoneCode?.Trim();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to resolve phoneCode by country prefix for user {UserId}", user.Id);
                }

                if (phoneCode == null && user.CompanyId.HasValue && user.CompanyId.Value != Guid.Empty)
                {
                    try
                    {
                        var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
                        var company = await companyRepo.GetByIdAsync(user.CompanyId.Value, cancellationToken);
                        if (company != null && company.CountryId != Guid.Empty)
                        {
                            var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                            var companyCountry = await countryRepo.GetByIdAsync(company.CountryId, cancellationToken);
                            phoneCode = companyCountry?.PhoneCode?.Trim();
                        }
                    }
                    catch {  }
                }

                if (phoneCode == null && addresses.Count > 0)
                {
                    var def = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses[0];
                    phoneCode = def.CountryPhoneCode;
                }

                if (phoneCode == null && cleanPhone.StartsWith("+"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(cleanPhone, @"^(\+\d{1,4})");
                    if (match.Success)
                    {
                        phoneCode = match.Value;
                    }
                }
            }

            var profile = new UserProfileDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                PhoneCode = phoneCode,
                ProfilePictureName = user.ProfilePictureName,
                UserType = user.UserType,
                CompanyId = user.CompanyId,
                Company = await LoadCompanyAsync(user, cancellationToken),
                Language = user.Language,
                IsEmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                Roles = roles,
                Addresses = addresses
            };

            return Result<UserProfileDto>.Success(profile, LocalizationKeys.Auth.ProfileFetched);
        }

        private async Task<CompanyDto?> LoadCompanyAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            if (user.CompanyId.HasValue && user.CompanyId.Value != Guid.Empty)
            {
                try
                {
                    var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
                    var company = await companyRepo.GetByIdAsync(user.CompanyId.Value, cancellationToken);
                    if (company != null && !company.IsDeleted)
                    {
                        string? countryNameEn = null;
                        string? countryNameAr = null;
                        try
                        {
                            var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                            var country = await countryRepo.GetByIdAsync(company.CountryId, cancellationToken);
                            countryNameEn = country?.NameEn;
                            countryNameAr = country?.NameAr;
                        }
                        catch {  }

                        return new CompanyDto
                        {
                            Id = company.Id,
                            Name = company.Name,
                            Email = company.Email,
                            Type = company.Type,
                            CountryId = company.CountryId,
                            CountryNameEn = countryNameEn,
                            CountryNameAr = countryNameAr,
                            Status = company.Status,
                            AccountManagerId = company.AccountManagerId,
                            IsActive = company.IsActive,
                            CreatedAt = company.CreatedAt,
                            UpdatedAt = company.UpdatedAt
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load company {CompanyId} for profile", user.CompanyId);
                }
            }

if (user.UserType == UserType.OrganizationUser)
            {
                try
                {
                    var distRepo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
                    var email = (user.Email ?? "").Trim().ToLower();
                    var app = await distRepo.GetAll(d => !d.IsDeleted && (d.ContactEmail.ToLower() == email || d.CreatedBy.ToLower() == email))
                        .OrderByDescending(d => d.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (app != null)
                    {
                        string? countryNameEn = null;
                        string? countryNameAr = null;
                        try
                        {
                            var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                            var country = await countryRepo.GetByIdAsync(app.CountryId, cancellationToken);
                            countryNameEn = country?.NameEn;
                            countryNameAr = country?.NameAr;
                        }
                        catch {  }

                        return new CompanyDto
                        {
                            Id = app.Id,
                            Name = app.CompanyName,
                            Email = app.ContactEmail,
                            Type = app.Type,
                            CountryId = app.CountryId,
                            CountryNameEn = countryNameEn,
                            CountryNameAr = countryNameAr,
                            Status = app.Status == DistributorApplicationStatus.Approved ? CompanyStatus.Approved :
                                     app.Status == DistributorApplicationStatus.Rejected ? CompanyStatus.Rejected :
                                     CompanyStatus.Pending,
                            CreatedAt = app.CreatedAt,
                            UpdatedAt = app.UpdatedAt
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load distributor application for user {UserId}", user.Id);
                }
            }

            return null;
        }
    }
}
