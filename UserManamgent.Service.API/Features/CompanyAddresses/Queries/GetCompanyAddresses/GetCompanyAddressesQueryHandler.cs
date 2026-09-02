using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.CompanyAddresses.Queries.GetCompanyAddresses
{
    public class GetCompanyAddressesQueryHandler : IRequestHandler<GetCompanyAddressesQuery, Result<IReadOnlyList<CompanyAddressDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCompanyAddressesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<CompanyAddressDto>>> Handle(GetCompanyAddressesQuery request, CancellationToken cancellationToken)
        {
            var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
            var company = await companyRepo.GetByIdAsync(request.CompanyId, cancellationToken);
            if (company == null || company.IsDeleted)
                return Result<IReadOnlyList<CompanyAddressDto>>.NotFound(LocalizationKeys.Company.NotFound);

            var repo = _unitOfWork.GetRepository<CompanyAddress, Guid>();
            var addresses = await repo
                .GetAllWithIncluding(a => a.CompanyId == request.CompanyId && !a.IsDeleted, a => a.Country, a => a.City, a => a.Zone)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
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
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<CompanyAddressDto>>.Success(addresses, LocalizationKeys.UserAddress.AddressesFetched);
        }
    }
}
