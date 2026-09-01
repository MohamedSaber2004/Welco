using Content.Services.API.ContentRoutes;
using Content.Services.API.Features.HelpArticles.Commands.CreateHelpArticle;
using Content.Services.API.Features.HelpArticles.Commands.DeleteHelpArticle;
using Content.Services.API.Features.HelpArticles.Commands.UpdateHelpArticle;
using Content.Services.API.Features.HelpArticles.Queries.GetHelpArticleById;
using Content.Services.API.Features.HelpArticles.Queries.GetHelpArticleBySlug;
using Content.Services.API.Features.HelpArticles.Queries.GetHelpArticles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Content.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(ContentApiRoutes.HelpArticles.Base)]
    public class HelpArticlesController : AppControllerBase
    {
        public HelpArticlesController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [Route(ContentApiRoutes.HelpArticles.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetHelpArticlesQuery { CategoryId = categoryId }, ct));

        [HttpGet]
        [Route(ContentApiRoutes.HelpArticles.GetById)]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetHelpArticleByIdQuery { Id = id }, ct));

        [HttpGet]
        [Route(ContentApiRoutes.HelpArticles.GetBySlug)]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySlug([FromRoute] string slug, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetHelpArticleBySlugQuery { Slug = slug }, ct));

        [HttpPost]
        [Route(ContentApiRoutes.HelpArticles.Create)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Create([FromBody] CreateHelpArticleCommand cmd, CancellationToken ct) => ToActionResult(await _mediator.Send(cmd, ct));

        [HttpPut]
        [Route(ContentApiRoutes.HelpArticles.Update)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateHelpArticleCommand cmd, CancellationToken ct)
        {
            cmd.Id = id;
            return ToActionResult(await _mediator.Send(cmd, ct));
        }

        [HttpDelete]
        [Route(ContentApiRoutes.HelpArticles.Delete)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new DeleteHelpArticleCommand { Id = id }, ct));
    }
}
