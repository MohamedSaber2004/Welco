using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Products.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<PaginatedResult<ProductDto>>
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
        public string? Material { get; set; }
        public decimal? LengthMin { get; set; }
        public decimal? LengthMax { get; set; }
        public Guid? CategoryId { get; set; }
        public bool? IsActive { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public bool? InStockOnly { get; set; }
        public Guid? CurrencyId { get; set; }
        public string? SortBy { get; set; } 
    }
}
