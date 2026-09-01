using Commerce.Services.API.CommerceRoutes;
using Commerce.Services.API.Features.Orders.Commands.CreateOrder;
using Commerce.Services.API.Features.Orders.Commands.UpdateOrderStatus;
using Commerce.Services.API.Features.Orders.Queries.GetOrderById;
using Commerce.Services.API.Features.Orders.Queries.GetOrders;
using Commerce.Services.API.Features.Orders.Queries.TrackOrder;
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
    [Route(CommerceApiRoutes.Orders.Base)]
    public class OrdersController : AppControllerBase
    {
        public OrdersController(IMediator mediator) : base(mediator) { }

        /// <summary>
        /// Get All Orders
        /// </summary>
        [HttpGet]
        [Route(CommerceApiRoutes.Orders.GetAll)]
        [RoleAuthorize(UserType.Admin)]
        public async Task<IActionResult> GetAll([FromQuery] GetOrdersQuery q, CancellationToken ct) => ToActionResult(await _mediator.Send(q, ct));

        /// <summary>
        /// Get Order By Id
        /// </summary>
        [HttpGet]
        [Route(CommerceApiRoutes.Orders.GetById)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetOrderByIdQuery { Id = id }, ct));

        /// <summary>
        /// Track Order by Order Number (public, no auth required)
        /// </summary>
        [HttpGet]
        [Route(CommerceApiRoutes.Orders.Track)]
        [AllowAnonymous]
        public async Task<IActionResult> Track([FromRoute] string orderNumber, CancellationToken ct)
            => ToActionResult(await _mediator.Send(new TrackOrderQuery { OrderNumber = orderNumber }, ct));

        /// <summary>
        /// Create Order
        /// </summary>
        [HttpPost]
        [Route(CommerceApiRoutes.Orders.Create)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand cmd, CancellationToken ct) => ToActionResult(await _mediator.Send(cmd, ct));

        /// <summary>
        /// Update Order Status
        /// </summary>
        [HttpPut]
        [Route(CommerceApiRoutes.Orders.UpdateStatus)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateOrderStatusCommand cmd, CancellationToken ct)
        {
            cmd.Id = id;
            return ToActionResult(await _mediator.Send(cmd, ct));
        }
    }
}
