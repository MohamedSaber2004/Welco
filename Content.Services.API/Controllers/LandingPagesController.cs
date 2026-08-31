using Content.Services.API.ContentRoutes;
using Content.Services.API.Features.LandingPages.Commands.CreateLandingPage;
using Content.Services.API.Features.LandingPages.Queries.GetLandingPageBySlug;
using Content.Services.API.Features.LandingPages.Queries.GetLandingPages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Content.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(ContentApiRoutes.LandingPages.Base)]
    public class LandingPagesController : AppControllerBase
    {
        public LandingPagesController(IMediator mediator) : base(mediator) { }

        /// <summary>
        /// Get All LandingPages
        /// </summary>
        [HttpGet]
        [Route(ContentApiRoutes.LandingPages.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] GetLandingPagesQuery q, CancellationToken ct) => ToActionResult(await _mediator.Send(q, ct));

        /// <summary>
        /// Get LandingPage By Slug
        /// </summary>
        [HttpGet]
        [Route(ContentApiRoutes.LandingPages.GetBySlug)]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySlug([FromRoute] string slug, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetLandingPageBySlugQuery { Slug = slug }, ct));

        /// <summary>
        /// Create LandingPage
        /// </summary>
        [HttpPost]
        [Route(ContentApiRoutes.LandingPages.Create)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Create([FromBody] CreateLandingPageCommand cmd, CancellationToken ct) => ToActionResult(await _mediator.Send(cmd, ct));
    }
}
