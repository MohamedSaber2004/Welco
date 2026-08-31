using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CurrencyEntity = Welco.Shared.Domain.Models.Currency;

namespace Product.Services.API.Features.Currencies.Queries.GetCurrencyById
{
    public class GetCurrencyByIdQueryHandler : IRequestHandler<GetCurrencyByIdQuery, Result<CurrencyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCurrencyByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CurrencyDto>> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
        {
            var currencyRepo = _unitOfWork.GetRepository<CurrencyEntity, Guid>();
            var currency = await currencyRepo.GetByIdAsync(request.Id, cancellationToken);

            if (currency == null || currency.IsDeleted)
            {
                return Result<CurrencyDto>.NotFound(LocalizationKeys.Currency.NotFound);
            }

            return Result<CurrencyDto>.Success(ToDto(currency), LocalizationKeys.Currency.Fetched);
        }

        internal static CurrencyDto ToDto(CurrencyEntity currency)
        {
            return new CurrencyDto
            {
                Id = currency.Id,
                NameEn = currency.NameEn,
                NameAr = currency.NameAr,
                Code = currency.Code,
                Symbol = currency.Symbol,
                IsActive = currency.IsActive,
                CreatedAt = currency.CreatedAt,
                UpdatedAt = currency.UpdatedAt
            };
        }
    }
}
