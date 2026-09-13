using Auth.Services.API.Features.Integration.Token;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Controllers;

namespace Auth.Services.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/integration/token")]
    public class IntegrationTokenController : AppControllerBase
    {
        public IntegrationTokenController(IMediator mediator) : base(mediator)
        {
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Issue([FromBody] CreateIntegrationTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }
    }
}
