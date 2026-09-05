using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using Sales.Services.API.Features.Shared;
using QuoteEntity = Welco.Shared.Domain.Models.Quote;
namespace Sales.Services.API.Features.Quotes.Queries.GetQuoteById
{
    public class GetQuoteByIdQueryHandler : IRequestHandler<GetQuoteByIdQuery, Result<QuoteDto>>
    {
        private readonly IUnitOfWork _uow; private readonly ICurrentUserService _cur;
        public GetQuoteByIdQueryHandler(IUnitOfWork uow, ICurrentUserService cur) { _uow = uow; _cur = cur; }
        public async Task<Result<QuoteDto>> Handle(GetQuoteByIdQuery r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<QuoteEntity, Guid>();
            var q = await repo.GetAll(x => !x.IsDeleted && x.Id == r.Id).Include(x => x.Items).ThenInclude(i => i.Product).Include(x => x.RFQ).FirstOrDefaultAsync(ct);
            if (q == null) return Result<QuoteDto>.NotFound(LocalizationKeys.Quote.NotFound);
            var caller = await BuyerScope.GetAsync(_uow, _cur, ct);
            if (caller.IsOrganizationUser && (q.RFQ == null || q.RFQ.CompanyId != caller.CompanyId)) return Result<QuoteDto>.NotFound(LocalizationKeys.Quote.NotFound);
            return Result<QuoteDto>.Success(new QuoteDto { Id = q.Id, QuoteNumber = q.QuoteNumber, RFQId = q.RFQId, Amount = q.Amount, ValidUntil = q.ValidUntil, Status = q.Status.ToString(), CreatedAt = q.CreatedAt, Items = q.Items.Where(i => !i.IsDeleted).Select(i => new QuoteItemDto { Id = i.Id, QuoteId = i.QuoteId, ProductId = i.ProductId, ProductNameEn = i.Product != null ? i.Product.NameEn : null, ProductNameAr = i.Product != null ? i.Product.NameAr : null, Quantity = i.Quantity, UnitPrice = i.UnitPrice }).ToList() }, LocalizationKeys.Quote.Fetched);
        }
    }
}
