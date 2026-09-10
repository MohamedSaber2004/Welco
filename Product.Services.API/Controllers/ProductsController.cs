using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Services.API.Features.Products.Commands.CreateProduct;
using Product.Services.API.Features.Products.Commands.DeleteProduct;
using Product.Services.API.Features.Products.Commands.UpdateProduct;
using Product.Services.API.Features.Products.Commands.UpdateProductVideos;
using Product.Services.API.Features.Products.Queries.GetProductById;
using Product.Services.API.Features.Products.Queries.GetProducts;
using Product.Services.API.Features.Products.Queries.GetProductVideos;
using Product.Services.API.ProductRoutes;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Product.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(ProductApiRoutes.Products.Base)]
    public class ProductsController : AppControllerBase
    {
        public ProductsController(IMediator mediator) : base(mediator)
        {
        }

                [HttpGet]
        [Route(ProductApiRoutes.Products.GetAll)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetProductsQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return ToActionResult(result);
        }

                [HttpGet]
        [Route(ProductApiRoutes.Products.GetById)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProductByIdQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

[HttpGet]
        [Route(ProductApiRoutes.Products.GetVideos)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVideos([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProductVideosQuery { ProductId = id }, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPut]
        [Route(ProductApiRoutes.Products.UpdateVideos)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVideos([FromRoute] Guid id, [FromBody] UpdateProductVideosCommand command, CancellationToken cancellationToken)
        {
            command.ProductId = id;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPost]
        [Route(ProductApiRoutes.Products.Create)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPut]
        [Route(ProductApiRoutes.Products.Update)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpDelete]
        [Route(ProductApiRoutes.Products.Delete)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteProductCommand { Id = id }, cancellationToken);
            return ToActionResult(result);
        }
    }
}
