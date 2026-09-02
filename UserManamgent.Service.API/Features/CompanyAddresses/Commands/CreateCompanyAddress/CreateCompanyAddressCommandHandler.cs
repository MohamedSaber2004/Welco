using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.CompanyAddresses.Commands.CreateCompanyAddress
{
    public class CreateCompanyAddressCommandHandler : IRequestHandler<CreateCompanyAddressCommand, Result<CompanyAddressDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateCompanyAddressCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CompanyAddressDto>> Handle(CreateCompanyAddressCommand request, CancellationToken cancellationToken)
        {
            var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
            var company = await companyRepo.GetByIdAsync(request.CompanyId, cancellationToken);
            if (company == null || company.IsDeleted)
            {
                return Result<CompanyAddressDto>.NotFound(LocalizationKeys.Company.NotFound);
            }

            var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
            var country = await countryRepo.GetByIdAsync(request.CountryId, cancellationToken);
            if (country == null || country.IsDeleted)
            {
                return Result<CompanyAddressDto>.NotFound(LocalizationKeys.Country.NotFound);
            }

            var cityRepo = _unitOfWork.GetRepository<City, Guid>();
            var city = await cityRepo.GetByIdAsync(request.CityId, cancellationToken);
            if (city == null || city.IsDeleted || city.CountryId != request.CountryId)
            {
                return Result<CompanyAddressDto>.NotFound(LocalizationKeys.City.NotFound);
            }

            var zoneRepo = _unitOfWork.GetRepository<Zone, Guid>();
            var zone = await zoneRepo.GetByIdAsync(request.ZoneId, cancellationToken);
            if (zone == null || zone.IsDeleted || zone.CityId != request.CityId)
            {
                return Result<CompanyAddressDto>.NotFound(LocalizationKeys.Zone.NotFound);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            var addressRepo = _unitOfWork.GetRepository<CompanyAddress, Guid>();
            // Support many addresses same country or across countries — each has own CountryId
            var existing = await addressRepo.GetAllListAsync(a => a.CompanyId == request.CompanyId && !a.IsDeleted, cancellationToken);
            var isFirst = !existing.Any();
            var shouldBeDefault = request.IsDefault || isFirst;

            if (shouldBeDefault)
            {
                foreach (var ex in existing.Where(a => a.IsDefault))
                {
                    ex.IsDefault = false;
                    ex.MarkAsUpdated(currentUserId);
                }
            }

            var address = CompanyAddress.Create(
                request.CompanyId,
                request.CountryId,
                request.CityId,
                request.ZoneId,
                request.Street,
                request.Building,
                request.Floor,
                request.Apartment,
                currentUserId,
                shouldBeDefault);

            await addressRepo.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new CompanyAddressDto
            {
                Id = address.Id,
                CompanyId = address.CompanyId,
                CountryId = address.CountryId,
                CountryNameEn = country.NameEn,
                CountryNameAr = country.NameAr,
                CountryCode = country.Code,
                CountryPhoneCode = country.PhoneCode,
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
                IsDefault = address.IsDefault,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt
            };

            return Result<CompanyAddressDto>.Created(dto, LocalizationKeys.UserAddress.AddressCreated);
        }
    }
}
