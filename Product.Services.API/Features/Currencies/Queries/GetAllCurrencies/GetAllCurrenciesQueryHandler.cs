using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Results;
using CurrencyEntity = Welco.Shared.Domain.Models.Currency;

namespace Product.Services.API.Features.Currencies.Queries.GetAllCurrencies
{
    public class GetAllCurrenciesQueryHandler : IRequestHandler<GetAllCurrenciesQuery, Result<List<CurrencyDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllCurrenciesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<CurrencyDto>>> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
        {
            var currencyRepo = _unitOfWork.GetRepository<CurrencyEntity, Guid>();
            var list = await currencyRepo.GetAll(c => !c.IsDeleted)
                .AsNoTracking()
                .OrderBy(c => c.Code)
                .Select(c => new CurrencyDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                    Code = c.Code,
                    Symbol = c.Symbol,
                    SymbolNative = c.SymbolNative,
                    DecimalDigits = c.DecimalDigits,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync(cancellationToken);
            return Result<List<CurrencyDto>>.Success(list);
        }
    }
}
