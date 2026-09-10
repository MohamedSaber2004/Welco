using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sales.Services.API.Features.Integration.Quotes.Commands.CreateExternalQuote;
using Sales.Services.API.Features.Integration.Quotes.Commands.UpdateExternalQuoteStatus;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Controllers;

namespace Sales.Services.API.Controllers
{
        [ServiceAuth]
    [ApiController]
    [IntegrationRouteName("Quotes")]
    [Route("api/integration/quotes")]
    public class IntegrationQuotesController : AppControllerBase
    {
        public IntegrationQuotesController(IMediator mediator) : base(mediator) { }

                [HttpPost("")]
        public async Task<IActionResult> CreateExternalQuote(
            [FromBody] CreateExternalQuoteCommand command,
            CancellationToken ct)
            => ToActionResult(await _mediator.Send(command, ct));

                [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateQuoteStatus(
            [FromRoute] Guid id,
            [FromBody] UpdateExternalQuoteStatusCommand command,
            CancellationToken ct)
        {
            command.Id = id;
            return ToActionResult(await _mediator.Send(command, ct));
        }
    }
}
