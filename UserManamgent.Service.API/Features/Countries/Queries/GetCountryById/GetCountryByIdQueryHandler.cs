using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Countries.Queries.GetCountryById
{
    public class GetCountryByIdQueryHandler : IRequestHandler<GetCountryByIdQuery, Result<CountryDto>>
    {
        private static readonly HashSet<string> EurozoneCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            "AT", "BE", "CY", "EE", "FI", "FR", "DE", "GR", "IE", "IT",
            "LV", "LT", "LU", "MT", "NL", "PT", "SK", "SI", "ES", "EU"
        };

        private readonly IUnitOfWork _unitOfWork;

        public GetCountryByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CountryDto>> Handle(GetCountryByIdQuery request, CancellationToken cancellationToken)
        {
            var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
            var currencyRepo = _unitOfWork.GetRepository<Currency, Guid>();

            var country = await countryRepo.GetByIdAsync(request.Id, cancellationToken);
            if (country == null || country.IsDeleted)
            {
                return Result<CountryDto>.NotFound(LocalizationKeys.Country.NotFound);
            }

            Currency? curr = null;
            var codeUpper = country.Code?.Trim().ToUpperInvariant();
            if (!string.IsNullOrWhiteSpace(codeUpper))
            {
                if (EurozoneCodes.Contains(codeUpper))
                {
                    curr = await currencyRepo.GetAll(c => !c.IsDeleted && c.Code == "EUR")
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken);
                }
                else
                {
                    curr = await currencyRepo.GetAll(c => !c.IsDeleted && c.CountryCode == codeUpper)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken);

                    if (curr == null)
                    {
                        curr = await currencyRepo.GetAll(c => !c.IsDeleted && c.Code.StartsWith(codeUpper))
                            .AsNoTracking()
                            .FirstOrDefaultAsync(cancellationToken);
                    }
                }
            }

            var dto = new CountryDto
            {
                Id = country.Id,
                NameEn = country.NameEn,
                NameAr = country.NameAr,
                Code = country.Code,
                PhoneCode = country.PhoneCode,
                CurrencyCode = curr?.Code,
                CurrencySymbol = curr?.Symbol,
                CurrencySymbolNative = curr?.SymbolNative,
                IsActive = country.IsActive,
                CreatedAt = country.CreatedAt
            };

            return Result<CountryDto>.Success(dto, LocalizationKeys.Country.Fetched);
        }
    }
}
