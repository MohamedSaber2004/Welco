using Content.Services.API.ContentRoutes;
using Content.Services.API.Features.FAQs.Commands.CreateFAQ;
using Content.Services.API.Features.FAQs.Commands.DeleteFAQ;
using Content.Services.API.Features.FAQs.Commands.UpdateFAQ;
using Content.Services.API.Features.FAQs.Queries.GetFAQs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Content.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(ContentApiRoutes.Faqs.Base)]
    public class FAQController : AppControllerBase
    {
        public FAQController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [Route(ContentApiRoutes.Faqs.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken ct) => ToActionResult(await _mediator.Send(new GetFAQsQuery(), ct));

        [HttpPost]
        [Route(ContentApiRoutes.Faqs.Create)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Create([FromBody] CreateFAQCommand cmd, CancellationToken ct) => ToActionResult(await _mediator.Send(cmd, ct));

        [HttpPut]
        [Route(ContentApiRoutes.Faqs.Update)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateFAQCommand cmd, CancellationToken ct)
        {
            cmd.Id = id;
            return ToActionResult(await _mediator.Send(cmd, ct));
        }

        [HttpDelete]
        [Route(ContentApiRoutes.Faqs.Delete)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new DeleteFAQCommand { Id = id }, ct));
    }
}
