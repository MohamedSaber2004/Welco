using MediatR;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Carts.Commands.CreateCart
{
    public class CreateCartCommand : IRequest<Result<CartDto>>
    {
        public Guid? UserId { get; set; }
        public string? SessionId { get; set; }
        public Guid? CurrencyId { get; set; }
    }
}
