using Commerce.Services.API.CommerceRoutes;
using Commerce.Services.API.Features.Carts.Commands.AddCartItem;
using Commerce.Services.API.Features.Carts.Commands.ClearCart;
using Commerce.Services.API.Features.Carts.Commands.CreateCart;
using Commerce.Services.API.Features.Carts.Commands.RemoveCartItem;
using Commerce.Services.API.Features.Carts.Commands.UpdateCartItem;
using Commerce.Services.API.Features.Carts.Queries.GetCartById;
using Commerce.Services.API.Features.Carts.Queries.GetCarts;
using Commerce.Services.API.Features.Carts.Queries.GetCartBySession;
using Commerce.Services.API.Features.Carts.Queries.GetCartByUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Commerce.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(CommerceApiRoutes.Carts.Base)]
    public class CartsController : AppControllerBase
    {
        public CartsController(IMediator mediator) : base(mediator) { }

        /// <summary>
        /// Get All Carts
        /// </summary>
        [HttpGet]
        [Route(CommerceApiRoutes.Carts.Create)]
        [RoleAuthorize(UserType.Admin)]
        public async Task<IActionResult> GetAll([FromQuery] GetCartsQuery q, CancellationToken ct) => ToActionResult(await _mediator.Send(q, ct));

        /// <summary>
        /// Get Cart By Id
        /// </summary>
        [HttpGet]
        [Route(CommerceApiRoutes.Carts.GetById)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetCartByIdQuery { Id = id }, ct));

        /// <summary>
        /// Get Cart By User
        /// </summary>
        [HttpGet]
        [Route(CommerceApiRoutes.Carts.GetByUser)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin)]
        public async Task<IActionResult> GetByUser([FromRoute] Guid userId, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetCartByUserQuery { UserId = userId }, ct));

        /// <summary>
        /// Get Cart By Session
        /// </summary>
        [HttpGet]
        [Route(CommerceApiRoutes.Carts.GetBySession)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin)]
        public async Task<IActionResult> GetBySession([FromRoute] string sessionId, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetCartBySessionQuery { SessionId = sessionId }, ct));

        /// <summary>
        /// Create Cart
        /// </summary>
        [HttpPost]
        [Route(CommerceApiRoutes.Carts.Create)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateCartCommand cmd, CancellationToken ct) => ToActionResult(await _mediator.Send(cmd, ct));

        /// <summary>
        /// Add Item to Cart
        /// </summary>
        [HttpPost]
        [Route(CommerceApiRoutes.Carts.AddItem)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin)]
        public async Task<IActionResult> AddItem([FromRoute] Guid id, [FromBody] AddCartItemCommand cmd, CancellationToken ct)
        {
            cmd.CartId = id;
            return ToActionResult(await _mediator.Send(cmd, ct));
        }

        /// <summary>
        /// Update Cart Item
        /// </summary>
        [HttpPut]
        [Route(CommerceApiRoutes.Carts.UpdateItem)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin)]
        public async Task<IActionResult> UpdateItem([FromRoute] Guid id, [FromRoute] Guid itemId, [FromBody] UpdateCartItemCommand cmd, CancellationToken ct)
        {
            cmd.CartId = id;
            cmd.ItemId = itemId;
            return ToActionResult(await _mediator.Send(cmd, ct));
        }

        /// <summary>
        /// Remove Cart Item
        /// </summary>
        [HttpDelete]
        [Route(CommerceApiRoutes.Carts.RemoveItem)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin)]
        public async Task<IActionResult> RemoveItem([FromRoute] Guid id, [FromRoute] Guid itemId, CancellationToken ct) => ToActionResult(await _mediator.Send(new RemoveCartItemCommand { CartId = id, ItemId = itemId }, ct));

        /// <summary>
        /// Clear Cart
        /// </summary>
        [HttpPost]
        [Route(CommerceApiRoutes.Carts.Clear)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin)]
        public async Task<IActionResult> Clear([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new ClearCartCommand { CartId = id }, ct));
    }
}
