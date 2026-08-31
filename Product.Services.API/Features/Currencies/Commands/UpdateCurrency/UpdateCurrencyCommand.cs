using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Currencies.Commands.UpdateCurrency
{
    public class UpdateCurrencyCommand : IRequest<Result<CurrencyDto>>
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
    }
}
