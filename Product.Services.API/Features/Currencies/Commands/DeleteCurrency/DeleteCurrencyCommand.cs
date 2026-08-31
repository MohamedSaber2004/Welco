using MediatR;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Currencies.Commands.DeleteCurrency
{
    public class DeleteCurrencyCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
