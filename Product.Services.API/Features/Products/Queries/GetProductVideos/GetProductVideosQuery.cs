using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Products.Queries.GetProductVideos
{
    public class GetProductVideosQuery : IRequest<Result<IReadOnlyList<ProductMediaDto>>>
    {
        public Guid ProductId { get; set; }
    }
}
