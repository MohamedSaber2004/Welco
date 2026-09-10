using Commerce.Services.API.CommerceRoutes;
using Commerce.Services.API.Features.Integration.Orders.Commands.CreateExternalOrder;
using Commerce.Services.API.Features.Integration.Orders.Commands.UpdateExternalOrderStatus;
using Commerce.Services.API.Features.Integration.Orders.Queries.GetExternalOrderById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Controllers;

namespace Commerce.Services.API.Controllers
{
        [ServiceAuth]
    [ApiController]
    [IntegrationRouteName("Orders")]
    [Route(IntegrationApiRoutes.Orders.Base)]
    public class IntegrationOrdersController : AppControllerBase
    {
        public IntegrationOrdersController(IMediator mediator) : base(mediator) { }

                [HttpPost(IntegrationApiRoutes.Orders.Create)]
        public async Task<IActionResult> CreateExternalOrder(
            [FromBody] CreateExternalOrderCommand command,
            CancellationToken ct)
            => ToActionResult(await _mediator.Send(command, ct));

                [HttpGet(IntegrationApiRoutes.Orders.GetById)]
        public async Task<IActionResult> GetOrder(
            [FromRoute] Guid id,
            CancellationToken ct)
            => ToActionResult(await _mediator.Send(new GetExternalOrderByIdQuery { Id = id }, ct));

                [HttpPut(IntegrationApiRoutes.Orders.UpdateStatus)]
        public async Task<IActionResult> UpdateOrderStatus(
            [FromRoute] Guid id,
            [FromBody] UpdateExternalOrderStatusCommand command,
            CancellationToken ct)
        {
            command.Id = id;
            return ToActionResult(await _mediator.Send(command, ct));
        }
    }
}
