using MediatR;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using RFQEntity = Welco.Shared.Domain.Models.RFQ;
using RFQItemEntity = Welco.Shared.Domain.Models.RFQItem;
namespace Sales.Services.API.Features.RFQs.Commands.CreateRFQ
{
    public class CreateRFQCommandHandler : IRequestHandler<CreateRFQCommand, Result<RFQDto>>
    {
        private readonly IUnitOfWork _uow; private readonly ICurrentUserService _cur;
        public CreateRFQCommandHandler(IUnitOfWork uow, ICurrentUserService cur) { _uow = uow; _cur = cur; }
        public async Task<Result<RFQDto>> Handle(CreateRFQCommand r, CancellationToken ct)
        {
            var companyRepo = _uow.GetRepository<Welco.Shared.Domain.Models.Company, Guid>();
            if (!await companyRepo.ExistsAsync(c => !c.IsDeleted && c.Id == r.CompanyId, ct)) return Result<RFQDto>.NotFound(LocalizationKeys.Company.NotFound);
            if (r.Items == null || !r.Items.Any()) return Result<RFQDto>.BadRequest(LocalizationKeys.RFQ.ItemsRequired);
            var curId = _cur.UserId != Guid.Empty ? _cur.UserId.ToString() : "System";
            var rfq = new RFQEntity { Id = Guid.NewGuid(), RFQNumber = $"RFQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}", CompanyId = r.CompanyId, Status = Welco.Shared.Domain.Models.RFQStatus.Pending };
            rfq.MarkAsCreated(curId);
            foreach (var it in r.Items)
            {
                var item = new RFQItemEntity { Id = Guid.NewGuid(), RFQId = rfq.Id, ProductId = it.ProductId, Quantity = it.Quantity, UnitPrice = it.UnitPrice < 0 ? 0 : it.UnitPrice, Notes = it.Notes };
                item.MarkAsCreated(curId);
                rfq.Items.Add(item);
            }
            var repo = _uow.GetRepository<RFQEntity, Guid>();
            await repo.AddAsync(rfq, ct); await _uow.SaveChangesAsync(ct);
            return Result<RFQDto>.Created(new RFQDto { Id = rfq.Id, RFQNumber = rfq.RFQNumber, CompanyId = rfq.CompanyId, Status = rfq.Status.ToString(), CreatedAt = rfq.CreatedAt, Items = rfq.Items.Select(i => new RFQItemDto { Id = i.Id, RFQId = i.RFQId, ProductId = i.ProductId, Quantity = i.Quantity, UnitPrice = i.UnitPrice, Notes = i.Notes }).ToList() }, LocalizationKeys.RFQ.Created);
        }
    }
}
