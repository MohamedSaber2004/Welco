using MediatR;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Wishlist.Commands.RemoveFromWishlist
{
    public class RemoveFromWishlistCommand : IRequest<Result<string>>
    {
        public Guid ProductId { get; set; }
    }
}
