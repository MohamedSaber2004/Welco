using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Wishlist.Queries.GetWishlist
{
    public class GetWishlistQuery : IRequest<Result<IReadOnlyList<ProductDto>>>
    {
    }
}
