using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Currencies.Commands.CreateCurrency
{
    public class CreateCurrencyCommand : IRequest<Result<CurrencyDto>>
    {
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
    }
}
