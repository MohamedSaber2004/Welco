using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Currencies.Queries.GetAllCurrencies
{
    public class GetAllCurrenciesQuery : IRequest<Result<List<CurrencyDto>>>
    {
    }
}
