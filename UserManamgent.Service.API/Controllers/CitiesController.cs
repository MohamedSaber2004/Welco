using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManamgent.Service.API.Features.Cities.Commands.CreateCity;
using UserManamgent.Service.API.Features.Cities.Commands.DeleteCity;
using UserManamgent.Service.API.Features.Cities.Commands.UpdateCity;
using UserManamgent.Service.API.Features.Cities.Queries.GetCities;
using UserManamgent.Service.API.Features.Cities.Queries.GetCityById;
using UserManamgent.Service.API.UserManagementRoutes;
using Microsoft.AspNetCore.Authorization;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace UserManamgent.Service.API.Controllers
{
    [RoleAuthorize]
    [Route(UserManagementApiRoutes.Cities.Base)]
    public class CitiesController : AppControllerBase
    {
        public CitiesController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(UserManagementApiRoutes.Cities.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? countryId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCitiesQuery { CountryId = countryId }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(UserManagementApiRoutes.Cities.GetByCountry)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCountry([FromRoute] Guid countryId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCitiesQuery { CountryId = countryId }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(UserManagementApiRoutes.Cities.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCityByIdQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [Route(UserManagementApiRoutes.Cities.Create)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateCityCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut]
        [Route(UserManagementApiRoutes.Cities.Update)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCityCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete]
        [Route(UserManagementApiRoutes.Cities.Delete)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCityCommand { Id = id }, cancellationToken);
            return ToActionResult(result);
        }
    }
}
