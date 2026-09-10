using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManamgent.Service.API.Features.Addresses.Commands.CreateAddress;
using UserManamgent.Service.API.Features.Addresses.Commands.DeleteAddress;
using UserManamgent.Service.API.Features.Addresses.Commands.UpdateAddress;
using UserManamgent.Service.API.Features.Addresses.Queries.GetAddressById;
using UserManamgent.Service.API.Features.Addresses.Queries.GetUserAddresses;
using UserManamgent.Service.API.UserManagementRoutes;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;

namespace UserManamgent.Service.API.Controllers
{
    [RoleAuthorize]
    [Route(UserManagementApiRoutes.Addresses.Base)]
    public class AddressesController : AppControllerBase
    {
        public AddressesController(IMediator mediator) : base(mediator)
        {
        }

                [HttpGet]
        [Route(UserManagementApiRoutes.Addresses.GetAllByUser)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllByUser([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetUserAddressesQuery { UserId = userId }, cancellationToken);
            return ToActionResult(result);
        }

                [HttpGet]
        [Route(UserManagementApiRoutes.Addresses.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAddressByIdQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPost]
        [Route(UserManagementApiRoutes.Addresses.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateAddressCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPut]
        [Route(UserManagementApiRoutes.Addresses.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateAddressCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpDelete]
        [Route(UserManagementApiRoutes.Addresses.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteAddressCommand { Id = id }, cancellationToken);
            return ToActionResult(result);
        }
    }
}
