using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Products.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<PaginatedResult<ProductDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? Sku { get; set; }
        public string? Material { get; set; }
        public decimal? LengthMin { get; set; }
        public decimal? LengthMax { get; set; }
        public Guid? CategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
}
