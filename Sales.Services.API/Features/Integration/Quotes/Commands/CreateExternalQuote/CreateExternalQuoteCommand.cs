using MediatR;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Results;

namespace Sales.Services.API.Features.Integration.Quotes.Commands.CreateExternalQuote
{
    public class CreateExternalQuoteCommand : IRequest<Result<ExternalQuoteResponse>>
    {
        public string? SourceMarket { get; set; } = "Egypt";
        public List<ExternalQuoteItemRequest> Items { get; set; } = new();
    }
}
