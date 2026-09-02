using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Services.API.Features.Wishlist.Commands.AddToWishlist;
using Product.Services.API.Features.Wishlist.Commands.RemoveFromWishlist;
using Product.Services.API.Features.Wishlist.Queries.GetWishlist;
using Product.Services.API.Features.Wishlist.Queries.IsInWishlist;
using Product.Services.API.ProductRoutes;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;

namespace Product.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(ProductApiRoutes.Wishlist.Base)]
    public class WishlistController : AppControllerBase
    {
        public WishlistController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [Route(ProductApiRoutes.Wishlist.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetWishlistQuery(), cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [Route(ProductApiRoutes.Wishlist.Add)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Add([FromRoute] Guid productId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new AddToWishlistCommand { ProductId = productId }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete]
        [Route(ProductApiRoutes.Wishlist.Remove)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Remove([FromRoute] Guid productId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RemoveFromWishlistCommand { ProductId = productId }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet]
        [Route(ProductApiRoutes.Wishlist.Check)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Check([FromRoute] Guid productId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new IsInWishlistQuery { ProductId = productId }, cancellationToken);
            return ToActionResult(result);
        }
    }
}
