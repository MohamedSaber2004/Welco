using MediatR;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Carts.Commands.AddCartItem
{
    public class AddCartItemCommand : IRequest<Result<CartDto>>
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPriceSnapshot { get; set; }
    }
}
