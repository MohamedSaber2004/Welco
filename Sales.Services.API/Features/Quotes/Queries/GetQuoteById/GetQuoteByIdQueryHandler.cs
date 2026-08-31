using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using QuoteEntity = Welco.Shared.Domain.Models.Quote;
namespace Sales.Services.API.Features.Quotes.Queries.GetQuoteById
{
    public class GetQuoteByIdQueryHandler : IRequestHandler<GetQuoteByIdQuery, Result<QuoteDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetQuoteByIdQueryHandler(IUnitOfWork uow) => _uow = uow;
        public async Task<Result<QuoteDto>> Handle(GetQuoteByIdQuery r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<QuoteEntity, Guid>();
            var q = await repo.GetAll(x => !x.IsDeleted && x.Id == r.Id).Select(x => new QuoteDto { Id = x.Id, QuoteNumber = x.QuoteNumber, RFQId = x.RFQId, Amount = x.Amount, ValidUntil = x.ValidUntil, Status = x.Status.ToString(), CreatedAt = x.CreatedAt }).FirstOrDefaultAsync(ct);
            if (q == null) return Result<QuoteDto>.NotFound(LocalizationKeys.Quote.NotFound);
            return Result<QuoteDto>.Success(q, LocalizationKeys.Quote.Fetched);
        }
    }
}
