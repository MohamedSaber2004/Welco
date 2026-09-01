using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManamgent.Service.API.Features.Companies.Commands.CreateCompany;
using UserManamgent.Service.API.Features.Companies.Commands.DeleteCompany;
using UserManamgent.Service.API.Features.Companies.Commands.UpdateCompany;
using UserManamgent.Service.API.Features.Companies.Queries.GetCompanies;
using UserManamgent.Service.API.Features.Companies.Queries.GetCompanyById;
using UserManamgent.Service.API.Features.Companies.Queries.GetMyCompany;
using UserManamgent.Service.API.UserManagementRoutes;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace UserManamgent.Service.API.Controllers
{
    [RoleAuthorize]
    [Route(UserManagementApiRoutes.Companies.Base)]
    public class CompaniesController : AppControllerBase
    {
        public CompaniesController(IMediator mediator) : base(mediator) { }
        [HttpGet][Route(UserManagementApiRoutes.Companies.GetMyCompany)] public async Task<IActionResult> GetMyCompany(CancellationToken ct) => ToActionResult(await _mediator.Send(new GetMyCompanyQuery(), ct));
        [HttpGet][Route(UserManagementApiRoutes.Companies.GetAll)] public async Task<IActionResult> GetAll([FromQuery] GetCompaniesQuery q, CancellationToken ct) => ToActionResult(await _mediator.Send(q, ct));
        [HttpGet][Route(UserManagementApiRoutes.Companies.GetById)] public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetCompanyByIdQuery { Id = id }, ct));
        [HttpPost][Route(UserManagementApiRoutes.Companies.Create)][RoleAuthorize(UserType.Admin)] public async Task<IActionResult> Create([FromBody] CreateCompanyCommand c, CancellationToken ct) => ToActionResult(await _mediator.Send(c, ct));
        [HttpPut][Route(UserManagementApiRoutes.Companies.Update)][RoleAuthorize(UserType.Admin)] public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCompanyCommand c, CancellationToken ct) { c.Id = id; return ToActionResult(await _mediator.Send(c, ct)); }
        [HttpDelete][Route(UserManagementApiRoutes.Companies.Delete)][RoleAuthorize(UserType.Admin)] public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new DeleteCompanyCommand { Id = id }, ct));
    }
}
