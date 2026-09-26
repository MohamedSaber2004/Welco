using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Products.Queries.GetMostSellingProducts
{
    public class GetMostSellingProductsQuery : IRequest<Result<IReadOnlyList<ProductDto>>>
    {
        public int Limit { get; set; } = 7;
        /// <summary>Only count order items placed within the last N days. Default = 7.</summary>
        public int DaysWindow { get; set; } = 7;
    }
}
