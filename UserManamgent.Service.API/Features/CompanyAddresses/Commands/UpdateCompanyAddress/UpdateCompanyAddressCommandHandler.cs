using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.CompanyAddresses.Commands.UpdateCompanyAddress
{
    public class UpdateCompanyAddressCommandHandler : IRequestHandler<UpdateCompanyAddressCommand, Result<CompanyAddressDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCompanyAddressCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CompanyAddressDto>> Handle(UpdateCompanyAddressCommand request, CancellationToken cancellationToken)
        {
            var addressRepo = _unitOfWork.GetRepository<CompanyAddress, Guid>();
            var address = await addressRepo.GetByIdAsync(request.Id, cancellationToken);
            if (address == null || address.IsDeleted)
            {
                return Result<CompanyAddressDto>.NotFound(LocalizationKeys.UserAddress.AddressNotFound);
            }

            var targetCountryId = request.CountryId ?? address.CountryId;
            var targetCityId = request.CityId ?? address.CityId;
            var targetZoneId = request.ZoneId ?? address.ZoneId;

            if (request.CountryId.HasValue)
            {
                var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                var country = await countryRepo.GetByIdAsync(request.CountryId.Value, cancellationToken);
                if (country == null || country.IsDeleted)
                    return Result<CompanyAddressDto>.NotFound(LocalizationKeys.Country.NotFound);
            }

            if (request.CityId.HasValue || request.CountryId.HasValue)
            {
                var cityRepo = _unitOfWork.GetRepository<City, Guid>();
                var city = await cityRepo.GetByIdAsync(targetCityId, cancellationToken);
                if (city == null || city.IsDeleted || city.CountryId != targetCountryId)
                    return Result<CompanyAddressDto>.NotFound(LocalizationKeys.City.NotFound);
            }

            if (request.ZoneId.HasValue || request.CityId.HasValue)
            {
                var zoneRepo = _unitOfWork.GetRepository<Zone, Guid>();
                var zone = await zoneRepo.GetByIdAsync(targetZoneId, cancellationToken);
                if (zone == null || zone.IsDeleted || zone.CityId != targetCityId)
                    return Result<CompanyAddressDto>.NotFound(LocalizationKeys.Zone.NotFound);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            if (request.IsDefault == true)
            {
                var others = await addressRepo.GetAllListAsync(a => a.CompanyId == address.CompanyId && a.Id != address.Id && !a.IsDeleted && a.IsDefault, cancellationToken);
                foreach (var o in others)
                {
                    o.IsDefault = false;
                    o.MarkAsUpdated(currentUserId);
                }
            }

            address.Update(
                request.CountryId,
                request.CityId,
                request.ZoneId,
                request.Street,
                request.Building,
                request.Floor,
                request.Apartment,
                currentUserId,
                request.IsDefault);

            addressRepo.Update(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await addressRepo
                .GetAllWithIncluding(a => a.Id == address.Id, a => a.Country, a => a.City, a => a.Zone)
                .FirstOrDefaultAsync(cancellationToken);

            var dto = new CompanyAddressDto
            {
                Id = address.Id,
                CompanyId = address.CompanyId,
                CountryId = address.CountryId,
                CountryNameEn = updated?.Country?.NameEn,
                CountryNameAr = updated?.Country?.NameAr,
                CountryCode = updated?.Country?.Code,
                CountryPhoneCode = updated?.Country?.PhoneCode,
                CityId = address.CityId,
                CityNameEn = updated?.City?.NameEn,
                CityNameAr = updated?.City?.NameAr,
                ZoneId = address.ZoneId,
                ZoneNameEn = updated?.Zone?.NameEn,
                ZoneNameAr = updated?.Zone?.NameAr,
                Street = address.Street,
                Building = address.Building,
                Floor = address.Floor,
                Apartment = address.Apartment,
                IsDefault = address.IsDefault,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt
            };

            return Result<CompanyAddressDto>.Success(dto, LocalizationKeys.UserAddress.AddressUpdated);
        }
    }
}
