using MediatR;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Results;
namespace Sales.Services.API.Features.Quotes.Queries.GetQuotes
{
    public class GetQuotesQuery : IRequest<PaginatedResult<QuoteDto>> { public int PageNumber { get; set; } = 1; public int PageSize { get; set; } = 10; }
}
