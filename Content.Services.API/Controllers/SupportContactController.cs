using Content.Services.API.ContentRoutes;
using Content.Services.API.Features.SupportContact.Commands.UpdateSupportContact;
using Content.Services.API.Features.SupportContact.Queries.GetSupportContact;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Content.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(ContentApiRoutes.SupportContact.Base)]
    public class SupportContactController : AppControllerBase
    {
        public SupportContactController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [Route(ContentApiRoutes.SupportContact.Get)]
        [AllowAnonymous]
        public async Task<IActionResult> Get(CancellationToken ct)
            => ToActionResult(await _mediator.Send(new GetSupportContactQuery(), ct));

        [HttpPut]
        [Route(ContentApiRoutes.SupportContact.Update)]
        [RoleAuthorize(UserType.Admin)]
        public async Task<IActionResult> Update([FromBody] UpdateSupportContactCommand cmd, CancellationToken ct)
            => ToActionResult(await _mediator.Send(cmd, ct));
    }
}
