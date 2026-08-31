using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sales.Services.API.Features.Quotes.Commands.ApproveQuote;
using Sales.Services.API.Features.Quotes.Commands.CreateQuote;
using Sales.Services.API.Features.Quotes.Commands.DeclineQuote;
using Sales.Services.API.Features.Quotes.Queries.GetQuoteById;
using Sales.Services.API.Features.Quotes.Queries.GetQuotes;
using Sales.Services.API.Features.RFQs.Commands.CreateRFQ;
using Sales.Services.API.Features.RFQs.Commands.UpdateRFQStatus;
using Sales.Services.API.Features.RFQs.Queries.GetRFQById;
using Sales.Services.API.Features.RFQs.Queries.GetRFQs;
using Sales.Services.API.SalesRoutes;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Sales.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(SalesApiRoutes.RFQs.Base)]
    public class RFQsController : AppControllerBase
    {
        public RFQsController(IMediator mediator) : base(mediator) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] GetRFQsQuery q, CancellationToken ct) => ToActionResult(await _mediator.Send(q, ct));
        [HttpGet][Route(SalesApiRoutes.RFQs.GetById)] public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetRFQByIdQuery { Id = id }, ct));
        [HttpPost][RoleAuthorize(UserType.OrganizationUser, UserType.Admin)] public async Task<IActionResult> Create([FromBody] CreateRFQCommand c, CancellationToken ct) => ToActionResult(await _mediator.Send(c, ct));
        [HttpPut][Route(SalesApiRoutes.RFQs.UpdateStatus)][RoleAuthorize(UserType.WelcoStaff, UserType.Admin)] public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateRFQStatusCommand c, CancellationToken ct) { c.Id = id; return ToActionResult(await _mediator.Send(c, ct)); }
    }
    [RoleAuthorize]
    [Route(SalesApiRoutes.Quotes.Base)]
    public class QuotesController : AppControllerBase
    {
        public QuotesController(IMediator mediator) : base(mediator) { }
        [HttpGet] public async Task<IActionResult> GetAll([FromQuery] GetQuotesQuery q, CancellationToken ct) => ToActionResult(await _mediator.Send(q, ct));
        [HttpGet][Route(SalesApiRoutes.Quotes.GetById)] public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetQuoteByIdQuery { Id = id }, ct));
        [HttpPost][RoleAuthorize(UserType.WelcoStaff, UserType.Admin)] public async Task<IActionResult> Create([FromBody] CreateQuoteCommand c, CancellationToken ct) => ToActionResult(await _mediator.Send(c, ct));
        [HttpPost][Route(SalesApiRoutes.Quotes.Approve)][RoleAuthorize(UserType.OrganizationUser, UserType.Admin)] public async Task<IActionResult> Approve([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new ApproveQuoteCommand { Id = id }, ct));
        [HttpPost][Route(SalesApiRoutes.Quotes.Decline)][RoleAuthorize(UserType.OrganizationUser, UserType.Admin)] public async Task<IActionResult> Decline([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new DeclineQuoteCommand { Id = id }, ct));
    }
}
