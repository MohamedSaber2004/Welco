using MediatR;
using Welco.Shared.Results;
namespace Sales.Services.API.Features.Quotes.Commands.CreateQuote
{
    public class CreateQuoteCommand : IRequest<Result<string>> { public Guid? RFQId { get; set; } public decimal Amount { get; set; } public DateTime ValidUntil { get; set; } public List<CreateQuoteItemDto> Items { get; set; } = new(); }
    public class CreateQuoteItemDto { public Guid ProductId { get; set; } public int Quantity { get; set; } public decimal UnitPrice { get; set; } }
}
