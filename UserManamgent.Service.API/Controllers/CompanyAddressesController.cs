using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManamgent.Service.API.Features.CompanyAddresses.Commands.CreateCompanyAddress;
using UserManamgent.Service.API.Features.CompanyAddresses.Commands.DeleteCompanyAddress;
using UserManamgent.Service.API.Features.CompanyAddresses.Commands.UpdateCompanyAddress;
using UserManamgent.Service.API.Features.CompanyAddresses.Queries.GetCompanyAddresses;
using UserManamgent.Service.API.Features.CompanyAddresses.Queries.GetCompanyAddressById;
using UserManamgent.Service.API.UserManagementRoutes;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;

namespace UserManamgent.Service.API.Controllers
{
    [RoleAuthorize]
    [Route(UserManagementApiRoutes.CompanyAddresses.Base)]
    public class CompanyAddressesController : AppControllerBase
    {
        public CompanyAddressesController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromRoute] Guid companyId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCompanyAddressesQuery { CompanyId = companyId }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet]
        [Route(UserManagementApiRoutes.CompanyAddresses.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid companyId, [FromRoute] Guid addressId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCompanyAddressByIdQuery { Id = addressId }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromRoute] Guid companyId, [FromBody] CreateCompanyAddressCommand command, CancellationToken cancellationToken)
        {
            command.CompanyId = companyId;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut]
        [Route(UserManagementApiRoutes.CompanyAddresses.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromRoute] Guid companyId, [FromRoute] Guid addressId, [FromBody] UpdateCompanyAddressCommand command, CancellationToken cancellationToken)
        {
            command.Id = addressId;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete]
        [Route(UserManagementApiRoutes.CompanyAddresses.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete([FromRoute] Guid companyId, [FromRoute] Guid addressId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCompanyAddressCommand { Id = addressId }, cancellationToken);
            return ToActionResult(result);
        }
    }

    [Route(UserManagementApiRoutes.CompanyAddresses.DirectBase)]
    public class DirectCompanyAddressesController : AppControllerBase
    {
        public DirectCompanyAddressesController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [Route(UserManagementApiRoutes.CompanyAddresses.DirectGetById)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCompanyAddressByIdQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut]
        [Route(UserManagementApiRoutes.CompanyAddresses.DirectUpdate)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCompanyAddressCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete]
        [Route(UserManagementApiRoutes.CompanyAddresses.DirectDelete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCompanyAddressCommand { Id = id }, cancellationToken);
            return ToActionResult(result);
        }
    }
}
