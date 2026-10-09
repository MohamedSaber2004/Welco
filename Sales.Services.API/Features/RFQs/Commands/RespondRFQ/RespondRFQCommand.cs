using MediatR;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Results;

namespace Sales.Services.API.Features.RFQs.Commands.RespondRFQ
{
    public class RespondRFQCommand : IRequest<Result<RFQDto>>
    {
        public Guid Id { get; set; }
        public string ResponseNote { get; set; } = string.Empty;
        public decimal? ProposedAmount { get; set; }
        public int? ValidityDays { get; set; }
        public List<RespondRFQItemDto>? Items { get; set; }
    }

    public class RespondRFQItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
