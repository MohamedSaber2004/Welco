using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Services.API.Features.Currencies.Commands.CreateCurrency;
using Product.Services.API.Features.Currencies.Commands.DeleteCurrency;
using Product.Services.API.Features.Currencies.Commands.UpdateCurrency;
using Product.Services.API.Features.Currencies.Queries.GetCurrencies;
using Product.Services.API.Features.Currencies.Queries.GetCurrencyById;
using Product.Services.API.ProductRoutes;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Product.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(ProductApiRoutes.Currencies.Base)]
    public class CurrenciesController : AppControllerBase
    {
        public CurrenciesController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Get All Currencies
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ProductApiRoutes.Currencies.GetAll)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetCurrenciesQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Get Currency By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ProductApiRoutes.Currencies.GetById)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCurrencyByIdQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Create Currency
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ProductApiRoutes.Currencies.Create)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateCurrencyCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Update Currency
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [Route(ProductApiRoutes.Currencies.Update)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCurrencyCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Delete Currency
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route(ProductApiRoutes.Currencies.Delete)]
        [RoleAuthorize(UserType.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCurrencyCommand { Id = id }, cancellationToken);
            return ToActionResult(result);
        }
    }
}
