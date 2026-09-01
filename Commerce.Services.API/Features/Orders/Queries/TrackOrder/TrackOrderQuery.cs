using MediatR;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Orders.Queries.TrackOrder
{
    public class TrackOrderQuery : IRequest<Result<OrderDto>>
    {
        public string OrderNumber { get; set; } = string.Empty;
    }
}
