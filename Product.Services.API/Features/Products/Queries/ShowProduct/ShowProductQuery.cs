using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Products.Queries.ShowProduct
{
    public class ShowProductQuery : IRequest<Result<ProductDto>>
    {
        public Guid Id { get; set; }
    }
}
