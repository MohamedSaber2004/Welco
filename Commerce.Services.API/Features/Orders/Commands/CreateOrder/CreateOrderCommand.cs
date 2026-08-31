using MediatR;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<Result<OrderDto>>
    {
        public Guid? UserId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? CurrencyId { get; set; }
        public Guid? QuoteId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
