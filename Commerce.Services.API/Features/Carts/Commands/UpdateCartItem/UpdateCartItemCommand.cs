using MediatR;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Carts.Commands.UpdateCartItem
{
    public class UpdateCartItemCommand : IRequest<Result<CartDto>>
    {
        public Guid CartId { get; set; }
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
