using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Currencies.Queries.GetCurrencyById
{
    public class GetCurrencyByIdQuery : IRequest<Result<CurrencyDto>>
    {
        public Guid Id { get; set; }
    }
}
