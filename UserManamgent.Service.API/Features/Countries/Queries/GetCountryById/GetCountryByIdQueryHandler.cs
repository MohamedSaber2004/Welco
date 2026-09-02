using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Countries.Queries.GetCountryById
{
    public class GetCountryByIdQueryHandler : IRequestHandler<GetCountryByIdQuery, Result<CountryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCountryByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CountryDto>> Handle(GetCountryByIdQuery request, CancellationToken cancellationToken)
        {
            var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
            var country = await countryRepo.GetByIdAsync(request.Id, cancellationToken);
            if (country == null || country.IsDeleted)
            {
                return Result<CountryDto>.NotFound(LocalizationKeys.Country.NotFound);
            }

            var dto = new CountryDto
            {
                Id = country.Id,
                NameEn = country.NameEn,
                NameAr = country.NameAr,
                Code = country.Code,
                PhoneCode = country.PhoneCode,
                IsActive = country.IsActive,
                CreatedAt = country.CreatedAt
            };

            return Result<CountryDto>.Success(dto, LocalizationKeys.Country.Fetched);
        }
    }
}
