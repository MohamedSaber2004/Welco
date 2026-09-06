using Content.Services.API.ContentRoutes;
using Content.Services.API.Features.OemServices.Queries.GetOemServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Controllers;

namespace Content.Services.API.Controllers
{
    [Route(ContentApiRoutes.OemServices.Base)]
    public class OemServicesController : AppControllerBase
    {
        public OemServicesController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [Route(ContentApiRoutes.OemServices.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => ToActionResult(await _mediator.Send(new GetOemServicesQuery(), ct));
    }
}
