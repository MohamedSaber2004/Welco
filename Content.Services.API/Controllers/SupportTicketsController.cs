using Content.Services.API.ContentRoutes;
using Content.Services.API.Features.SupportTickets.Commands.CloseTicket;
using Content.Services.API.Features.SupportTickets.Commands.CreateTicket;
using Content.Services.API.Features.SupportTickets.Commands.ReplyTicket;
using Content.Services.API.Features.SupportTickets.Queries.GetMyTickets;
using Content.Services.API.Features.SupportTickets.Queries.GetTicketById;
using Content.Services.API.Features.SupportTickets.Queries.GetTickets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Content.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(ContentApiRoutes.SupportTickets.Base)]
    public class SupportTicketsController : AppControllerBase
    {
        public SupportTicketsController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [Route(ContentApiRoutes.SupportTickets.GetAll)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null, CancellationToken ct = default)
            => ToActionResult(await _mediator.Send(new GetTicketsQuery { PageNumber = pageNumber, PageSize = pageSize, Status = status }, ct));

        [HttpGet]
        [Route(ContentApiRoutes.SupportTickets.GetMy)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> GetMy(CancellationToken ct) => ToActionResult(await _mediator.Send(new GetMyTicketsQuery(), ct));

        [HttpGet]
        [Route(ContentApiRoutes.SupportTickets.GetById)]
        [RoleAuthorize]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetTicketByIdQuery { Id = id }, ct));

        [HttpPost]
        [Route(ContentApiRoutes.SupportTickets.Create)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Create([FromBody] CreateTicketCommand cmd, CancellationToken ct) => ToActionResult(await _mediator.Send(cmd, ct));

        [HttpPost]
        [Route(ContentApiRoutes.SupportTickets.Reply)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Reply([FromRoute] Guid id, [FromBody] ReplyTicketCommand cmd, CancellationToken ct)
        {
            cmd.Id = id;
            return ToActionResult(await _mediator.Send(cmd, ct));
        }

        [HttpPost]
        [Route(ContentApiRoutes.SupportTickets.Close)]
        [RoleAuthorize]
        public async Task<IActionResult> Close([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new CloseTicketCommand { Id = id }, ct));
    }
}
