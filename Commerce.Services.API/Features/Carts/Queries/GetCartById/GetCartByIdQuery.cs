using MediatR;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Carts.Queries.GetCartById
{
    public class GetCartByIdQuery : IRequest<Result<CartDto>>
    {
        public Guid Id { get; set; }
    }
}
