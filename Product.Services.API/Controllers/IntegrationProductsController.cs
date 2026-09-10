using MediatR;
using Microsoft.AspNetCore.Mvc;
using Product.Services.API.Features.Integration.Categories.Queries.GetExternalCategories;
using Product.Services.API.Features.Integration.Categories.Queries.GetExternalCategoryById;
using Product.Services.API.Features.Integration.Products.Queries.GetExternalProductById;
using Product.Services.API.Features.Integration.Products.Queries.GetExternalProducts;
using Product.Services.API.Features.Integration.Providers.Queries.GetExternalProviders;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Controllers;

namespace Product.Services.API.Controllers
{
        [ServiceAuth]
    [ApiController]
    [IntegrationRouteName("Products")]
    [Route("api/integration/products")]
    public class IntegrationProductsController : AppControllerBase
    {
        public IntegrationProductsController(IMediator mediator) : base(mediator) { }

                [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
            => ToActionResult(await _mediator.Send(new GetExternalProductsQuery { Page = page, PageSize = pageSize }, ct));

                [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
            => ToActionResult(await _mediator.Send(new GetExternalProductByIdQuery { Id = id }, ct));
    }

        [ServiceAuth]
    [ApiController]
    [IntegrationRouteName("Providers")]
    [Route("api/integration/providers")]
    public class IntegrationProvidersController : AppControllerBase
    {
        public IntegrationProvidersController(IMediator mediator) : base(mediator) { }

                [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => ToActionResult(await _mediator.Send(new GetExternalProvidersQuery(), ct));
    }

        [ServiceAuth]
    [ApiController]
    [IntegrationRouteName("Categories")]
    [Route("api/integration/categories")]
    public class IntegrationCategoriesController : AppControllerBase
    {
        public IntegrationCategoriesController(IMediator mediator) : base(mediator) { }

                [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => ToActionResult(await _mediator.Send(new GetExternalCategoriesQuery(), ct));

                [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
            => ToActionResult(await _mediator.Send(new GetExternalCategoryByIdQuery { Id = id }, ct));
    }
}
