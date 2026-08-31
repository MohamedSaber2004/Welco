using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDetailsDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public GetUserByIdQueryHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UserDetailsDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null || user.IsDeleted)
            {
                return Result<UserDetailsDto>.NotFound(LocalizationKeys.UserManagement.UserNotFound);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var addressRepo = _unitOfWork.GetRepository<UserAddress, Guid>();
            var addresses = await addressRepo
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

            var userDetails = new UserDetailsDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureName = user.ProfilePictureName,
                UserType = user.UserType,
                CompanyId = user.CompanyId,
                Language = user.Language,
                IsActive = user.IsActive,
                IsEmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = roles,
                Addresses = addresses
            };

            return Result<UserDetailsDto>.Success(userDetails, LocalizationKeys.UserManagement.UserFetched);
        }
    }
}
