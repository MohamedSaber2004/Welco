using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Countries.Queries.GetCountries
{
    public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, Result<IReadOnlyList<CountryDto>>>
    {
        private static readonly HashSet<string> EurozoneCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            "AT", "BE", "CY", "EE", "FI", "FR", "DE", "GR", "IE", "IT",
            "LV", "LT", "LU", "MT", "NL", "PT", "SK", "SI", "ES", "EU"
        };

        private readonly IUnitOfWork _unitOfWork;

        public GetCountriesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<CountryDto>>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
        {
            var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
            var currencyRepo = _unitOfWork.GetRepository<Currency, Guid>();

            var currencies = await currencyRepo
                .GetAll(c => !c.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var currencyByCountryCode = currencies
                .Where(c => !string.IsNullOrWhiteSpace(c.CountryCode))
                .GroupBy(c => c.CountryCode!.Trim().ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var eurCurrency = currencies.FirstOrDefault(c => c.Code.Equals("EUR", StringComparison.OrdinalIgnoreCase));

            var countries = await countryRepo
                .GetAll(c => !c.IsDeleted)
                .OrderBy(c => c.NameEn)
                .ToListAsync(cancellationToken);

            var list = countries.Select(c =>
            {
                Currency? curr = null;
                var codeUpper = c.Code?.Trim().ToUpperInvariant();
                if (!string.IsNullOrWhiteSpace(codeUpper))
                {
                    if (EurozoneCodes.Contains(codeUpper) && eurCurrency != null)
                    {
                        curr = eurCurrency;
                    }
                    else if (!currencyByCountryCode.TryGetValue(codeUpper, out curr))
                    {
                        // Fallback: match currency whose 3-letter code starts with country's 2-letter code (e.g. US -> USD, EG -> EGP)
                        curr = currencies.FirstOrDefault(cu => cu.Code.StartsWith(codeUpper, StringComparison.OrdinalIgnoreCase));
                    }
                }

                return new CountryDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                    Code = c.Code,
                    PhoneCode = c.PhoneCode,
                    CurrencyCode = curr?.Code,
                    CurrencySymbol = curr?.Symbol,
                    CurrencySymbolNative = curr?.SymbolNative,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                };
            }).ToList();

            return Result<IReadOnlyList<CountryDto>>.Success(list, LocalizationKeys.Country.ListFetched);
        }
    }
}
