using MediatR;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Orders.Queries.GetOrders
{
    public class GetOrdersQuery : IRequest<PaginatedResult<OrderDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
