using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManamgent.Service.API.Features.AuditLogs.Queries.GetAuditLogs.GetAuditLogById;
using UserManamgent.Service.API.Features.AuditLogs.Queries.GetAuditLogs.GetAuditLogs;
using UserManamgent.Service.API.UserManagementRoutes;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace UserManamgent.Service.API.Controllers
{
    [RoleAuthorize]
    [Route(UserManagementApiRoutes.AuditLogs.Base)]
    public class AuditLogsController : AppControllerBase
    {
        public AuditLogsController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [Route(UserManagementApiRoutes.AuditLogs.GetAll)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> GetAll([FromQuery] GetAuditLogsQuery query, CancellationToken ct)
            => ToActionResult(await _mediator.Send(query, ct));

        [HttpGet]
        [Route(UserManagementApiRoutes.AuditLogs.GetById)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAuditLogByIdQuery { Id = id }, ct);
            return ToActionResult(result);
        }
    }
}
