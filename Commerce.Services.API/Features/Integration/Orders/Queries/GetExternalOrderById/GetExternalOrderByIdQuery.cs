using MediatR;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Integration.Orders.Queries.GetExternalOrderById
{
    public class GetExternalOrderByIdQuery : IRequest<Result<ExternalOrderResponse>>
    {
        public Guid Id { get; set; }
    }
}
