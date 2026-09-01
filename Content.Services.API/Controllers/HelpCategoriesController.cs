using Content.Services.API.ContentRoutes;
using Content.Services.API.Features.HelpCategories.Commands.CreateHelpCategory;
using Content.Services.API.Features.HelpCategories.Commands.DeleteHelpCategory;
using Content.Services.API.Features.HelpCategories.Commands.UpdateHelpCategory;
using Content.Services.API.Features.HelpCategories.Queries.GetHelpCategories;
using Content.Services.API.Features.HelpCategories.Queries.GetHelpCategoryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Content.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(ContentApiRoutes.HelpCategories.Base)]
    public class HelpCategoriesController : AppControllerBase
    {
        public HelpCategoriesController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [Route(ContentApiRoutes.HelpCategories.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken ct) => ToActionResult(await _mediator.Send(new GetHelpCategoriesQuery(), ct));

        [HttpGet]
        [Route(ContentApiRoutes.HelpCategories.GetById)]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetHelpCategoryByIdQuery { Id = id }, ct));

        [HttpPost]
        [Route(ContentApiRoutes.HelpCategories.Create)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Create([FromBody] CreateHelpCategoryCommand cmd, CancellationToken ct) => ToActionResult(await _mediator.Send(cmd, ct));

        [HttpPut]
        [Route(ContentApiRoutes.HelpCategories.Update)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateHelpCategoryCommand cmd, CancellationToken ct)
        {
            cmd.Id = id;
            return ToActionResult(await _mediator.Send(cmd, ct));
        }

        [HttpDelete]
        [Route(ContentApiRoutes.HelpCategories.Delete)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new DeleteHelpCategoryCommand { Id = id }, ct));
    }
}
