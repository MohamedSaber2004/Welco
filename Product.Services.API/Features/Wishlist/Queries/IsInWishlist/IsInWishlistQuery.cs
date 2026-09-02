using MediatR;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Wishlist.Queries.IsInWishlist
{
    public class IsInWishlistQuery : IRequest<Result<bool>>
    {
        public Guid ProductId { get; set; }
    }
}
