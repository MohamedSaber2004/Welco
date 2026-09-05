using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using Sales.Services.API.Features.Shared;
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
            // Organization users may only request for their own company.
            var caller = await BuyerScope.GetAsync(_uow, _cur, ct);
            if (caller.IsOrganizationUser && caller.CompanyId != r.CompanyId) return Result<RFQDto>.NotFound(LocalizationKeys.Company.NotFound);
            if (r.Items == null || !r.Items.Any()) return Result<RFQDto>.BadRequest(LocalizationKeys.RFQ.ItemsRequired);
            // Reject unknown products so lines always resolve to real catalog items.
            var productRepo = _uow.GetRepository<Welco.Shared.Domain.Models.Product, Guid>();
            var wantedIds = r.Items.Select(i => i.ProductId).Distinct().ToList();
            var found = await productRepo.GetAll(p => !p.IsDeleted && wantedIds.Contains(p.Id)).Select(p => p.Id).ToListAsync(ct);
            if (found.Count != wantedIds.Count) return Result<RFQDto>.NotFound(LocalizationKeys.Product.NotFound);
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
            var created = await repo.GetAll(x => x.Id == rfq.Id).Select(x => new RFQDto { Id = x.Id, RFQNumber = x.RFQNumber, CompanyId = x.CompanyId, Status = x.Status.ToString(), CreatedAt = x.CreatedAt, Items = x.Items.Where(i => !i.IsDeleted).Select(i => new RFQItemDto { Id = i.Id, RFQId = i.RFQId, ProductId = i.ProductId, ProductNameEn = i.Product != null ? i.Product.NameEn : null, ProductNameAr = i.Product != null ? i.Product.NameAr : null, ImageName = i.Product != null ? i.Product.ImageName : null, Quantity = i.Quantity, UnitPrice = i.UnitPrice, Notes = i.Notes }).ToList() }).FirstOrDefaultAsync(ct);
            return Result<RFQDto>.Created(created!, LocalizationKeys.RFQ.Created);
        }
    }
}
