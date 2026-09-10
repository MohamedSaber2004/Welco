using MediatR;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Integration.Products.Queries.GetExternalProducts
{
    public class GetExternalProductsQuery : IRequest<Result<List<ExternalProductDto>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
