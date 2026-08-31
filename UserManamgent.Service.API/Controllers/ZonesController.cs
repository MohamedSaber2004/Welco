using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManamgent.Service.API.Features.Zones.Commands.CreateZone;
using UserManamgent.Service.API.Features.Zones.Commands.DeleteZone;
using UserManamgent.Service.API.Features.Zones.Commands.UpdateZone;
using UserManamgent.Service.API.Features.Zones.Queries.GetZoneById;
using UserManamgent.Service.API.Features.Zones.Queries.GetZones;
using UserManamgent.Service.API.UserManagementRoutes;
using Microsoft.AspNetCore.Authorization;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace UserManamgent.Service.API.Controllers
{
    [RoleAuthorize]
    [Route(UserManagementApiRoutes.Zones.Base)]
    public class ZonesController : AppControllerBase
    {
        public ZonesController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(UserManagementApiRoutes.Zones.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? cityId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetZonesQuery { CityId = cityId }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(UserManagementApiRoutes.Zones.GetByCity)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCity([FromRoute] Guid cityId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetZonesQuery { CityId = cityId }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(UserManagementApiRoutes.Zones.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetZoneByIdQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [Route(UserManagementApiRoutes.Zones.Create)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateZoneCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut]
        [Route(UserManagementApiRoutes.Zones.Update)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateZoneCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete]
        [Route(UserManagementApiRoutes.Zones.Delete)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteZoneCommand { Id = id }, cancellationToken);
            return ToActionResult(result);
        }
    }
}
