using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using Sales.Services.API.Features.Shared;
using QuoteEntity = Welco.Shared.Domain.Models.Quote;
namespace Sales.Services.API.Features.Quotes.Queries.GetQuotes
{
    public class GetQuotesQueryHandler : IRequestHandler<GetQuotesQuery, PaginatedResult<QuoteDto>>
    {
        private readonly IUnitOfWork _uow; private readonly ICurrentUserService _cur;
        public GetQuotesQueryHandler(IUnitOfWork uow, ICurrentUserService cur) { _uow = uow; _cur = cur; }
        public async Task<PaginatedResult<QuoteDto>> Handle(GetQuotesQuery r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<QuoteEntity, Guid>();
            var q = repo.GetAll(x => !x.IsDeleted).AsNoTracking();
            
            var caller = await BuyerScope.GetAsync(_uow, _cur, ct);
            if (caller.IsOrganizationUser && caller.CompanyId.HasValue)
            {
                var companyId = caller.CompanyId.Value;
                q = q.Where(x => (x.RFQ != null && x.RFQ.CompanyId == companyId) || x.Items.Any(i => !i.IsDeleted && i.Product != null && i.Product.CompanyId == companyId) || (x.RFQ != null && x.RFQ.Items.Any(i => !i.IsDeleted && i.Product != null && i.Product.CompanyId == companyId)));
            }
            else if (caller.IsOrganizationUser)
            {
                q = q.Where(x => x.RFQ != null && x.RFQ.CompanyId == caller.CompanyId);
            }
            else if (caller.IsClient && caller.UserId.HasValue)
            {
                // Strict customer isolation: Client only sees quotes for their own RFQs
                var uid = caller.UserId.Value;
                var uidStr = uid.ToString();
                if (caller.CompanyId.HasValue)
                    q = q.Where(x => (x.RFQ != null && (x.RFQ.CreatedBy == uidStr || x.RFQ.CompanyId == caller.CompanyId.Value)) || x.CreatedBy == uidStr);
                else
                    q = q.Where(x => (x.RFQ != null && x.RFQ.CreatedBy == uidStr) || x.CreatedBy == uidStr);
            }
            return await q.OrderByDescending(x => x.CreatedAt).ToPaginatedListAsync(x => new QuoteDto
            {
                Id = x.Id,
                QuoteNumber = x.QuoteNumber,
                RFQId = x.RFQId,
                Amount = x.Amount,
                ValidUntil = x.ValidUntil,
                Status = x.Status.ToString(),
                Note = x.Note,
                CreatedAt = x.CreatedAt
            }, r.PageNumber, r.PageSize, LocalizationKeys.Quote.ListFetched, ct);
        }
    }
}
