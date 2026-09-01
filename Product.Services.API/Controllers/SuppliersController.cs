using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Services.API.Features.Suppliers.Queries.GetSupplierById;
using Product.Services.API.Features.Suppliers.Queries.GetSuppliers;
using Welco.Shared.Controllers;

namespace Product.Services.API.Controllers
{
    [Route("api/v1/suppliers")]
    public class SuppliersController : AppControllerBase
    {
        public SuppliersController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(CancellationToken ct) => ToActionResult(await _mediator.Send(new GetSuppliersQuery(), ct));

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetSupplierByIdQuery { Id = id }, ct));
    }
}
