using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Categories.Queries.GetCategoryProducts
{
    public class GetCategoryProductsQuery : IRequest<Result<List<ProductDto>>>
    {
        public Guid CategoryId { get; set; }
    }
}
