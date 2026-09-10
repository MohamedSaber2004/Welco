using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Results;
using RFQEntity = Welco.Shared.Domain.Models.RFQ;

namespace Sales.Services.API.Features.Integration.Quotes.Commands.UpdateExternalQuoteStatus
{
    public class UpdateExternalQuoteStatusCommandHandler : IRequestHandler<UpdateExternalQuoteStatusCommand, Result<bool>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UpdateExternalQuoteStatusCommandHandler> _logger;

        public UpdateExternalQuoteStatusCommandHandler(
            IUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UpdateExternalQuoteStatusCommandHandler> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateExternalQuoteStatusCommand request, CancellationToken ct)
        {
            _logger.LogInformation("[Integration MediatR] UpdateExternalQuoteStatus WelcoRfqId={WelcoRfqId} Status={Status}",
                request.Id, request.Status);

            if (!Enum.TryParse<RFQStatus>(request.Status, ignoreCase: true, out var newStatus))
            {
                return Result<bool>.BadRequest($"Invalid RFQ status value: {request.Status}");
            }

            var rfqRepo = _uow.GetRepository<RFQEntity, Guid>();
            var rfq = await rfqRepo.GetAll(r => !r.IsDeleted && r.Id == request.Id).FirstOrDefaultAsync(ct);

            if (rfq == null)
            {
                return Result<bool>.NotFound("RFQ not found.");
            }

            rfq.Status = newStatus;
            rfq.MarkAsUpdated("integration-service");
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("[Integration MediatR] RFQ status updated WelcoRfqId={WelcoRfqId} NewStatus={NewStatus}",
                request.Id, newStatus);

            return Result<bool>.Success(true);
        }
    }
}
