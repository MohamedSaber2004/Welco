using MediatR;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Wishlist.Commands.AddToWishlist
{
    public class AddToWishlistCommand : IRequest<Result<string>>
    {
        public Guid ProductId { get; set; }
    }
}
