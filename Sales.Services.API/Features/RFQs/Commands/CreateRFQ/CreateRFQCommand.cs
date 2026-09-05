using MediatR;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Results;
namespace Sales.Services.API.Features.RFQs.Commands.CreateRFQ
{
    public class CreateRFQCommand : IRequest<Result<RFQDto>>
    {
        public Guid CompanyId { get; set; }
        public List<CreateRFQItemDto> Items { get; set; } = new();
    }
    public class CreateRFQItemDto { public Guid ProductId { get; set; } public int Quantity { get; set; } public decimal UnitPrice { get; set; } public string? Notes { get; set; } }
}
