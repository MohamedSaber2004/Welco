using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManamgent.Service.API.Features.Countries.Commands.CreateCountry;
using UserManamgent.Service.API.Features.Countries.Commands.DeleteCountry;
using UserManamgent.Service.API.Features.Countries.Commands.UpdateCountry;
using UserManamgent.Service.API.Features.Countries.Queries.GetCountries;
using UserManamgent.Service.API.Features.Countries.Queries.GetCountryById;
using UserManamgent.Service.API.UserManagementRoutes;
using Microsoft.AspNetCore.Authorization;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace UserManamgent.Service.API.Controllers
{
    [RoleAuthorize]
    [Route(UserManagementApiRoutes.Countries.Base)]
    public class CountriesController : AppControllerBase
    {
        public CountriesController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [RoleAuthorize(UserType.Doctor, UserType.Admin)]
        [Route(UserManagementApiRoutes.Countries.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCountriesQuery(), cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet]
        [RoleAuthorize(UserType.Doctor, UserType.Admin)]
        [Route(UserManagementApiRoutes.Countries.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCountryByIdQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [Route(UserManagementApiRoutes.Countries.Create)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateCountryCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut]
        [Route(UserManagementApiRoutes.Countries.Update)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCountryCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete]
        [Route(UserManagementApiRoutes.Countries.Delete)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCountryCommand { Id = id }, cancellationToken);
            return ToActionResult(result);
        }
    }
}
