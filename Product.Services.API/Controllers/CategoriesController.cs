using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Services.API.Features.Categories.Commands.CreateCategory;
using Product.Services.API.Features.Categories.Commands.DeleteCategory;
using Product.Services.API.Features.Categories.Commands.UpdateCategory;
using Product.Services.API.Features.Categories.Queries.GetCategories;
using Product.Services.API.Features.Categories.Queries.GetCategoryById;
using Product.Services.API.Features.Categories.Queries.GetCategoryProducts;
using Product.Services.API.Features.Categories.Queries.ShowCategory;
using Product.Services.API.ProductRoutes;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Product.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(ProductApiRoutes.Categories.Base)]
    public class CategoriesController : AppControllerBase
    {
        public CategoriesController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Get All Categories
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ProductApiRoutes.Categories.GetAll)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetCategoriesQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Get Category By Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ProductApiRoutes.Categories.GetById)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Show Categories
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(ProductApiRoutes.Categories.Show)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Show([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ShowCategoryQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Create Category
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ProductApiRoutes.Categories.Create)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Update Category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [Route(ProductApiRoutes.Categories.Update)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Delete  Category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route(ProductApiRoutes.Categories.Delete)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCategoryCommand { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet]
        [Route(ProductApiRoutes.Categories.GetProductsByCategory)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductsByCategory([FromRoute] Guid categoryId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCategoryProductsQuery { CategoryId = categoryId }, cancellationToken);
            return ToActionResult(result);
        }
    }
}
