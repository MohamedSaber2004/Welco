using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Products.Queries.GetMostSellingProducts
{
    public class GetMostSellingProductsQuery : IRequest<Result<IReadOnlyList<ProductDto>>>
    {
        public int Limit { get; set; } = 7;
    }
}
