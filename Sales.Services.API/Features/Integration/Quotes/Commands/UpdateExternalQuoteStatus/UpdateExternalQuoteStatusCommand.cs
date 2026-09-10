using MediatR;
using Welco.Shared.Results;

namespace Sales.Services.API.Features.Integration.Quotes.Commands.UpdateExternalQuoteStatus
{
    public class UpdateExternalQuoteStatusCommand : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
    }
}
