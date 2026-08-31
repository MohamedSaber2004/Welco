using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using QuoteEntity = Welco.Shared.Domain.Models.Quote;
using QuoteItemEntity = Welco.Shared.Domain.Models.QuoteItem;
namespace Sales.Services.API.Features.Quotes.Commands.CreateQuote
{
    public class CreateQuoteCommandHandler : IRequestHandler<CreateQuoteCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow; private readonly ICurrentUserService _cur;
        public CreateQuoteCommandHandler(IUnitOfWork uow, ICurrentUserService cur) { _uow = uow; _cur = cur; }
        public async Task<Result<string>> Handle(CreateQuoteCommand r, CancellationToken ct)
        {
            var curId = _cur.UserId != Guid.Empty ? _cur.UserId.ToString() : "System";
            var quote = new QuoteEntity { Id = Guid.NewGuid(), QuoteNumber = $"QT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}", RFQId = r.RFQId, Amount = r.Amount, ValidUntil = r.ValidUntil, Status = Welco.Shared.Domain.Models.QuoteStatus.Draft, CreatedBySalesRepId = _cur.UserId };
            quote.MarkAsCreated(curId);
            foreach (var it in r.Items) { var qi = new QuoteItemEntity { Id = Guid.NewGuid(), QuoteId = quote.Id, ProductId = it.ProductId, Quantity = it.Quantity, UnitPrice = it.UnitPrice }; qi.MarkAsCreated(curId); quote.Items.Add(qi); }
            var repo = _uow.GetRepository<QuoteEntity, Guid>();
            await repo.AddAsync(quote, ct); await _uow.SaveChangesAsync(ct);
            return Result<string>.Created(quote.Id.ToString(), LocalizationKeys.Quote.Created);
        }
    }
}
