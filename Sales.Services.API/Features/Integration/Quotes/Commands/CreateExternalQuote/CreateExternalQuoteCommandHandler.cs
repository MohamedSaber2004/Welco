using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Results;
using RFQEntity = Welco.Shared.Domain.Models.RFQ;
using RFQItemEntity = Welco.Shared.Domain.Models.RFQItem;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Sales.Services.API.Features.Integration.Quotes.Commands.CreateExternalQuote
{
    public class CreateExternalQuoteCommandHandler : IRequestHandler<CreateExternalQuoteCommand, Result<ExternalQuoteResponse>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CreateExternalQuoteCommandHandler> _logger;

        public CreateExternalQuoteCommandHandler(
            IUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            ILogger<CreateExternalQuoteCommandHandler> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Result<ExternalQuoteResponse>> Handle(CreateExternalQuoteCommand request, CancellationToken ct)
        {
            _logger.LogInformation("[Integration MediatR] CreateExternalQuote Market={Market}", request.SourceMarket);

            if (request.Items == null || !request.Items.Any())
            {
                return Result<ExternalQuoteResponse>.BadRequest("Quote must contain at least one item.");
            }

var productRepo = _uow.GetRepository<ProductEntity, Guid>();
            foreach (var item in request.Items)
            {
                var exists = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Id == item.WelcoProductId, ct);
                if (!exists)
                {
                    return Result<ExternalQuoteResponse>.NotFound($"Product {item.WelcoProductId} not found.");
                }
            }

            var rfq = new RFQEntity
            {
                RFQNumber = $"RFQ-EG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                CompanyId = Guid.Empty, 
                Status = Welco.Shared.Domain.Models.RFQStatus.Pending,
                SourceMarket = request.SourceMarket ?? "Egypt"
            };
            rfq.MarkAsCreated("integration-service");

            foreach (var item in request.Items)
            {
                var rfqItem = new RFQItemEntity
                {
                    RFQId = rfq.Id,
                    ProductId = item.WelcoProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.RequestedUnitPrice ?? 0,
                    Notes = item.Notes
                };
                rfqItem.MarkAsCreated("integration-service");
                rfq.Items.Add(rfqItem);
            }

            var rfqRepo = _uow.GetRepository<RFQEntity, Guid>();
            await rfqRepo.AddAsync(rfq, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("[Integration MediatR] RFQ created WelcoRfqId={WelcoRfqId} WelcoRfqNumber={WelcoRfqNumber}",
                rfq.Id, rfq.RFQNumber);

            return Result<ExternalQuoteResponse>.Created(new ExternalQuoteResponse
            {
                WelcoRfqId = rfq.Id,
                WelcoRfqNumber = rfq.RFQNumber,
                Status = rfq.Status.ToString()
            });
        }
    }
}
