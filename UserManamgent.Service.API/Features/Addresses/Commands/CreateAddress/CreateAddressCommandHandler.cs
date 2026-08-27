using MediatR;
using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Addresses.Commands.CreateAddress
{
    public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, Result<UserAddressDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public CreateAddressCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<Result<UserAddressDto>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null || user.IsDeleted)
            {
                return Result<UserAddressDto>.NotFound(LocalizationKeys.UserManagement.UserNotFound);
            }

            var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
            var country = await countryRepo.GetByIdAsync(request.CountryId, cancellationToken);
            if (country == null || country.IsDeleted)
            {
                return Result<UserAddressDto>.NotFound(LocalizationKeys.Country.NotFound);
            }

            var cityRepo = _unitOfWork.GetRepository<City, Guid>();
            var city = await cityRepo.GetByIdAsync(request.CityId, cancellationToken);
            if (city == null || city.IsDeleted || city.CountryId != request.CountryId)
            {
                return Result<UserAddressDto>.NotFound(LocalizationKeys.City.NotFound);
            }

            var zoneRepo = _unitOfWork.GetRepository<Zone, Guid>();
            var zone = await zoneRepo.GetByIdAsync(request.ZoneId, cancellationToken);
            if (zone == null || zone.IsDeleted || zone.CityId != request.CityId)
            {
                return Result<UserAddressDto>.NotFound(LocalizationKeys.Zone.NotFound);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            var address = UserAddress.Create(
                request.UserId,
                request.CountryId,
                request.CityId,
                request.ZoneId,
                request.Street,
                request.Building,
                request.Floor,
                request.Apartment,
                currentUserId);

            var addressRepo = _unitOfWork.GetRepository<UserAddress, Guid>();
            await addressRepo.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var addressDto = new UserAddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                CountryId = address.CountryId,
                CountryNameEn = country.NameEn,
                CountryNameAr = country.NameAr,
                CityId = address.CityId,
                CityNameEn = city.NameEn,
                CityNameAr = city.NameAr,
                ZoneId = address.ZoneId,
                ZoneNameEn = zone.NameEn,
                ZoneNameAr = zone.NameAr,
                Street = address.Street,
                Building = address.Building,
                Floor = address.Floor,
                Apartment = address.Apartment,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt
            };

            return Result<UserAddressDto>.Created(addressDto, LocalizationKeys.UserAddress.AddressCreated);
        }
    }
}
