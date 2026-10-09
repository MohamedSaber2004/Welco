using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using Sales.Services.API.Features.Shared;
using RFQEntity = Welco.Shared.Domain.Models.RFQ;
using QuoteEntity = Welco.Shared.Domain.Models.Quote;
using QuoteItemEntity = Welco.Shared.Domain.Models.QuoteItem;

namespace Sales.Services.API.Features.RFQs.Commands.RespondRFQ
{
    public class RespondRFQCommandHandler : IRequestHandler<RespondRFQCommand, Result<RFQDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _cur;

        public RespondRFQCommandHandler(IUnitOfWork uow, ICurrentUserService cur)
        {
            _uow = uow;
            _cur = cur;
        }

        public async Task<Result<RFQDto>> Handle(RespondRFQCommand r, CancellationToken ct)
        {
            var rfqRepo = _uow.GetRepository<RFQEntity, Guid>();
            var rfq = await rfqRepo.GetAll(x => !x.IsDeleted && x.Id == r.Id)
                .Include(x => x.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(ct);

            if (rfq == null) return Result<RFQDto>.NotFound(LocalizationKeys.RFQ.NotFound);

            var caller = await BuyerScope.GetAsync(_uow, _cur, ct);
            if (caller.IsOrganizationUser && caller.CompanyId.HasValue)
            {
                var companyId = caller.CompanyId.Value;
                var owns = rfq.CompanyId == companyId || rfq.Items.Any(i => !i.IsDeleted && i.Product != null && i.Product.CompanyId == companyId);
                if (!owns) return Result<RFQDto>.NotFound(LocalizationKeys.RFQ.NotFound);
            }

            var curId = _cur.UserId != Guid.Empty ? _cur.UserId.ToString() : "System";
            if (!string.IsNullOrWhiteSpace(r.ResponseNote))
                rfq.ResponseNote = r.ResponseNote.Trim();
            rfq.MarkAsUpdated(curId);

            // If a proposed quote/amount is provided, create linked Quote
            if (r.ProposedAmount.HasValue && r.ProposedAmount.Value > 0)
            {
                var quote = new QuoteEntity
                {
                    Id = Guid.NewGuid(),
                    QuoteNumber = $"QT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                    RFQId = rfq.Id,
                    Amount = r.ProposedAmount.Value,
                    ValidUntil = DateTime.UtcNow.AddDays(r.ValidityDays.HasValue && r.ValidityDays.Value > 0 ? r.ValidityDays.Value : 14),
                    Status = QuoteStatus.Draft,
                    CreatedBySalesRepId = _cur.UserId,
                    Note = r.ResponseNote?.Trim()
                };
                quote.MarkAsCreated(curId);

                if (r.Items != null && r.Items.Any())
                {
                    foreach (var it in r.Items)
                    {
                        var qi = new QuoteItemEntity
                        {
                            Id = Guid.NewGuid(),
                            QuoteId = quote.Id,
                            ProductId = it.ProductId,
                            Quantity = it.Quantity,
                            UnitPrice = it.UnitPrice
                        };
                        qi.MarkAsCreated(curId);
                        quote.Items.Add(qi);
                    }
                }
                else
                {
                    foreach (var it in rfq.Items.Where(i => !i.IsDeleted))
                    {
                        var qi = new QuoteItemEntity
                        {
                            Id = Guid.NewGuid(),
                            QuoteId = quote.Id,
                            ProductId = it.ProductId,
                            Quantity = it.Quantity,
                            UnitPrice = it.UnitPrice
                        };
                        qi.MarkAsCreated(curId);
                        quote.Items.Add(qi);
                    }
                }

                var quoteRepo = _uow.GetRepository<QuoteEntity, Guid>();
                await quoteRepo.AddAsync(quote, ct);
                rfq.Status = RFQStatus.Quoted;
            }

            await _uow.SaveChangesAsync(ct);

            var dto = new RFQDto
            {
                Id = rfq.Id,
                RFQNumber = rfq.RFQNumber,
                CompanyId = rfq.CompanyId,
                Status = rfq.Status.ToString(),
                AssignedSalesRepId = rfq.AssignedSalesRepId,
                ResponseNote = rfq.ResponseNote,
                CreatedAt = rfq.CreatedAt,
                Items = rfq.Items.Where(i => !i.IsDeleted).Select(i => new RFQItemDto
                {
                    Id = i.Id,
                    RFQId = i.RFQId,
                    ProductId = i.ProductId,
                    ProductNameEn = i.Product != null ? i.Product.NameEn : null,
                    ProductNameAr = i.Product != null ? i.Product.NameAr : null,
                    ImageName = i.Product != null ? i.Product.ImageName : null,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Notes = i.Notes
                }).ToList()
            };

            return Result<RFQDto>.Success(dto, LocalizationKeys.RFQ.Updated);
        }
    }
}
