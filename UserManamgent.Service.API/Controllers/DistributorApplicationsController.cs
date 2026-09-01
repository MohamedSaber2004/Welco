using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManamgent.Service.API.Features.DistributorApplications.Commands.CreateDistributorApplication;
using UserManamgent.Service.API.UserManagementRoutes;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace UserManamgent.Service.API.Controllers
{
    [RoleAuthorize]
    [Route("api/v1/user-management/distributor-applications")]
    public class DistributorApplicationsController : AppControllerBase
    {
        public DistributorApplicationsController(IMediator mediator) : base(mediator) { }

        // POST /api/v1/user-management/distributor-applications — OrganizationUser + Admin
        [HttpPost]
        [Route("")]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateDistributorApplicationCommand cmd, CancellationToken ct)
            => ToActionResult(await _mediator.Send(cmd, ct));
    }
}
