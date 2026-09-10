using Commerce.Services.API.CommerceRoutes;
using Commerce.Services.API.Features.Integration.Inventory.Commands.ReserveInventory;
using Commerce.Services.API.Features.Integration.Inventory.Queries.CheckInventory;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Controllers;

namespace Commerce.Services.API.Controllers
{
        [ServiceAuth]
    [ApiController]
    [IntegrationRouteName("Inventory")]
    [Route(IntegrationApiRoutes.Inventory.Base)]
    public class IntegrationInventoryController : AppControllerBase
    {
        public IntegrationInventoryController(IMediator mediator) : base(mediator) { }

                [HttpPost(IntegrationApiRoutes.Inventory.Check)]
        public async Task<IActionResult> Check(
            [FromBody] CheckInventoryQuery query,
            CancellationToken ct)
            => ToActionResult(await _mediator.Send(query, ct));

                [HttpPost(IntegrationApiRoutes.Inventory.Reserve)]
        public async Task<IActionResult> Reserve(
            [FromBody] ReserveInventoryCommand command,
            CancellationToken ct)
            => ToActionResult(await _mediator.Send(command, ct));
    }
}
