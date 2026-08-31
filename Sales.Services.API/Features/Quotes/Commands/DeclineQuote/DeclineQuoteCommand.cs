using MediatR;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using QuoteEntity = Welco.Shared.Domain.Models.Quote;
namespace Sales.Services.API.Features.Quotes.Commands.DeclineQuote
{
    public class DeclineQuoteCommand : IRequest<Result<string>> { public Guid Id { get; set; } }
    public class DeclineQuoteCommandHandler : IRequestHandler<DeclineQuoteCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow;
        public DeclineQuoteCommandHandler(IUnitOfWork uow) => _uow = uow;
        public async Task<Result<string>> Handle(DeclineQuoteCommand r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<QuoteEntity, Guid>();
            var q = await repo.GetByIdAsync(r.Id, ct);
            if (q == null || q.IsDeleted) return Result<string>.NotFound(LocalizationKeys.Quote.NotFound);
            q.Status = Welco.Shared.Domain.Models.QuoteStatus.Declined; q.MarkAsUpdated("System"); repo.Update(q); await _uow.SaveChangesAsync(ct);
            return Result<string>.Success(q.Id.ToString(), LocalizationKeys.Quote.Declined);
        }
    }
}
