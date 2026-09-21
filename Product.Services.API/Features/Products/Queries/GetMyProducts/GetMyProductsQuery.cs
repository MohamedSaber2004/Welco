using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Products.Queries.GetMyProducts
{
    /// <summary>
    /// Provider's own catalog (mediator model). Company is resolved
    /// server-side from claims; providers only ever see their own items.
    /// </summary>
    public class GetMyProductsQuery : IRequest<PaginatedResult<ProductDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? Sku { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
