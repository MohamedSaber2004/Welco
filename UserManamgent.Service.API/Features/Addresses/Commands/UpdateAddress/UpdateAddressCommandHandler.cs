using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Addresses.Commands.UpdateAddress
{
    public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, Result<UserAddressDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateAddressCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<UserAddressDto>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            var addressRepo = _unitOfWork.GetRepository<UserAddress, Guid>();
            var address = await addressRepo.GetByIdAsync(request.Id, cancellationToken);
            if (address == null || address.IsDeleted)
            {
                return Result<UserAddressDto>.NotFound(LocalizationKeys.UserAddress.AddressNotFound);
            }

            var targetCountryId = request.CountryId ?? address.CountryId;
            var targetCityId = request.CityId ?? address.CityId;
            var targetZoneId = request.ZoneId ?? address.ZoneId;

            if (request.CountryId.HasValue)
            {
                var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                var country = await countryRepo.GetByIdAsync(request.CountryId.Value, cancellationToken);
                if (country == null || country.IsDeleted)
                {
                    return Result<UserAddressDto>.NotFound(LocalizationKeys.Country.NotFound);
                }
            }

            if (request.CityId.HasValue || request.CountryId.HasValue)
            {
                var cityRepo = _unitOfWork.GetRepository<City, Guid>();
                var city = await cityRepo.GetByIdAsync(targetCityId, cancellationToken);
                if (city == null || city.IsDeleted || city.CountryId != targetCountryId)
                {
                    return Result<UserAddressDto>.NotFound(LocalizationKeys.City.NotFound);
                }
            }

            if (request.ZoneId.HasValue || request.CityId.HasValue)
            {
                var zoneRepo = _unitOfWork.GetRepository<Zone, Guid>();
                var zone = await zoneRepo.GetByIdAsync(targetZoneId, cancellationToken);
                if (zone == null || zone.IsDeleted || zone.CityId != targetCityId)
                {
                    return Result<UserAddressDto>.NotFound(LocalizationKeys.Zone.NotFound);
                }
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            address.Update(
                request.CountryId,
                request.CityId,
                request.ZoneId,
                request.Street,
                request.Building,
                request.Floor,
                request.Apartment,
                currentUserId);

            addressRepo.Update(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedAddress = await addressRepo
                .GetAllWithIncluding(a => a.Id == address.Id, a => a.Country, a => a.City, a => a.Zone)
                .FirstOrDefaultAsync(cancellationToken);

            var addressDto = new UserAddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                CountryId = address.CountryId,
                CountryNameEn = updatedAddress?.Country?.NameEn,
                CountryNameAr = updatedAddress?.Country?.NameAr,
                CityId = address.CityId,
                CityNameEn = updatedAddress?.City?.NameEn,
                CityNameAr = updatedAddress?.City?.NameAr,
                ZoneId = address.ZoneId,
                ZoneNameEn = updatedAddress?.Zone?.NameEn,
                ZoneNameAr = updatedAddress?.Zone?.NameAr,
                Street = address.Street,
                Building = address.Building,
                Floor = address.Floor,
                Apartment = address.Apartment,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt
            };

            return Result<UserAddressDto>.Success(addressDto, LocalizationKeys.UserAddress.AddressUpdated);
        }
    }
}
