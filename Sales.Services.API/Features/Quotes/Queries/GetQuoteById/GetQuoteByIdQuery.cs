using MediatR;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Results;
namespace Sales.Services.API.Features.Quotes.Queries.GetQuoteById
{
    public class GetQuoteByIdQuery : IRequest<Result<QuoteDto>> { public Guid Id { get; set; } }
}
