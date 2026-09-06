using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Currencies.Queries.GetCurrencyByCode
{
    public class GetCurrencyByCodeQuery : IRequest<Result<CurrencyDto>>
    {
        public string Code { get; set; } = string.Empty;
    }
}
