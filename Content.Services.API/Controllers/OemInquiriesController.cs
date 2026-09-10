using Content.Services.API.ContentRoutes;
using Content.Services.API.Features.OemInquiries.Commands.CreateOemInquiry;
using Content.Services.API.Features.OemInquiries.Commands.DeleteOemInquiry;
using Content.Services.API.Features.OemInquiries.Queries.GetOemInquiries;
using Content.Services.API.Features.OemInquiries.Queries.GetOemInquiryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Content.Services.API.Controllers
{
    [Route(ContentApiRoutes.OemInquiries.Base)]
    public class OemInquiriesController : AppControllerBase
    {
        public OemInquiriesController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> GetAll([FromQuery] GetOemInquiriesQuery q, CancellationToken ct)
            => ToActionResult(await _mediator.Send(q, ct));

        [HttpGet]
        [Route(ContentApiRoutes.OemInquiries.GetById)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
            => ToActionResult(await _mediator.Send(new GetOemInquiryByIdQuery { Id = id }, ct));

[HttpPost]
        [Route(ContentApiRoutes.OemInquiries.Create)]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateOemInquiryCommand c, CancellationToken ct)
            => ToActionResult(await _mediator.Send(c, ct));

        [HttpDelete]
        [Route(ContentApiRoutes.OemInquiries.Delete)]
        [RoleAuthorize(UserType.Admin)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
            => ToActionResult(await _mediator.Send(new DeleteOemInquiryCommand { Id = id }, ct));
    }
}
