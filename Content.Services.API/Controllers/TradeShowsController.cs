using Content.Services.API.Features.TradeShows.Commands.CreateTradeShow;
using Content.Services.API.Features.TradeShows.Queries.GetTradeShows;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Content.Services.API.Controllers
{
    [Route("api/v1/trade-shows")]
    public class TradeShowsController : AppControllerBase
    {
        public TradeShowsController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] bool? upcomingOnly, CancellationToken ct)
            => ToActionResult(await _mediator.Send(new GetTradeShowsQuery { UpcomingOnly = upcomingOnly }, ct));

        [HttpPost]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Create([FromBody] CreateTradeShowCommand cmd, CancellationToken ct)
            => ToActionResult(await _mediator.Send(cmd, ct));
    }
}
