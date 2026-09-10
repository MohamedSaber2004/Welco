using MediatR;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Integration.Products.Queries.GetExternalProductById
{
    public class GetExternalProductByIdQuery : IRequest<Result<ExternalProductDto>>
    {
        public Guid Id { get; set; }
    }
}
