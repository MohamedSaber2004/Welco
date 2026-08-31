using MediatR;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Carts.Queries.GetCartByUser
{
    public class GetCartByUserQuery : IRequest<Result<CartDto>>
    {
        public Guid UserId { get; set; }
    }
}
