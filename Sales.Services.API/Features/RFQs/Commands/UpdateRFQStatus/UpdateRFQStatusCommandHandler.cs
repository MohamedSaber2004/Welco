using MediatR;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using RFQEntity = Welco.Shared.Domain.Models.RFQ;
namespace Sales.Services.API.Features.RFQs.Commands.UpdateRFQStatus
{
    public class UpdateRFQStatusCommandHandler : IRequestHandler<UpdateRFQStatusCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow;
        public UpdateRFQStatusCommandHandler(IUnitOfWork uow) => _uow = uow;
        public async Task<Result<string>> Handle(UpdateRFQStatusCommand r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<RFQEntity, Guid>();
            var rfq = await repo.GetByIdAsync(r.Id, ct);
            if (rfq == null || rfq.IsDeleted) return Result<string>.NotFound(LocalizationKeys.RFQ.NotFound);
            var normalized = (r.Status ?? string.Empty).Trim();
            // Backward-compat aliases sent by provider dashboard
            if (normalized.Equals("Declined", StringComparison.OrdinalIgnoreCase) || normalized.Equals("Decline", StringComparison.OrdinalIgnoreCase)) normalized = Welco.Shared.Domain.Models.RFQStatus.Cancelled.ToString();
            else if (normalized.Equals("Accepted", StringComparison.OrdinalIgnoreCase) || normalized.Equals("Accept", StringComparison.OrdinalIgnoreCase) || normalized.Equals("Approved", StringComparison.OrdinalIgnoreCase)) normalized = Welco.Shared.Domain.Models.RFQStatus.Quoted.ToString();
            if (Enum.TryParse<Welco.Shared.Domain.Models.RFQStatus>(normalized, true, out var st)) rfq.Status = st;
            else return Result<string>.BadRequest(LocalizationKeys.RFQ.InvalidStatus);
            rfq.MarkAsUpdated("System"); repo.Update(rfq); await _uow.SaveChangesAsync(ct);
            return Result<string>.Success(rfq.Id.ToString(), LocalizationKeys.RFQ.Updated);
        }
    }
}
