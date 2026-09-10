using MediatR;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Results;

namespace Commerce.Services.API.Features.Integration.Orders.Commands.CreateExternalOrder
{
    public class CreateExternalOrderCommand : IRequest<Result<ExternalOrderResponse>>
    {
        public string? SourceMarket { get; set; } = "Egypt";
        public Guid? ExternalCustomerId { get; set; }
        public Guid? CurrencyId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ExternalOrderItemRequest> Items { get; set; } = new();
    }
}
