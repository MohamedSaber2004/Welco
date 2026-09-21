using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Products.Queries.GetSkuProviders
{
    /// <summary>
    /// All providers offering one SKU (product details "Offered by").
    /// </summary>
    public class GetSkuProvidersQuery : IRequest<PaginatedResult<SkuProviderDto>>
    {
        public string Sku { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
