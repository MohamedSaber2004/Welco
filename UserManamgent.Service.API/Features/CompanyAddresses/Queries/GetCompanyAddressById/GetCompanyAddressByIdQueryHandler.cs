using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.CompanyAddresses.Queries.GetCompanyAddressById
{
    public class GetCompanyAddressByIdQueryHandler : IRequestHandler<GetCompanyAddressByIdQuery, Result<CompanyAddressDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCompanyAddressByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CompanyAddressDto>> Handle(GetCompanyAddressByIdQuery request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<CompanyAddress, Guid>();
            var address = await repo.GetAllWithIncluding(a => a.Id == request.Id && !a.IsDeleted, a => a.Country, a => a.City, a => a.Zone)
                .Select(a => new CompanyAddressDto
                {
                    Id = a.Id,
                    CompanyId = a.CompanyId,
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
                .FirstOrDefaultAsync(cancellationToken);

            if (address == null)
                return Result<CompanyAddressDto>.NotFound(LocalizationKeys.UserAddress.AddressNotFound);

            return Result<CompanyAddressDto>.Success(address, LocalizationKeys.UserAddress.AddressesFetched);
        }
    }
}
