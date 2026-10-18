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

        private string? _searchTerm;
        public string? SearchTerm
        {
            get => !string.IsNullOrWhiteSpace(_searchTerm)
                ? _searchTerm
                : (!string.IsNullOrWhiteSpace(Search) ? Search : (!string.IsNullOrWhiteSpace(Q) ? Q : Query));
            set => _searchTerm = value;
        }

        public string? Search { get; set; }
        public string? Q { get; set; }
        public string? Query { get; set; }

        public string? Sku { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
