using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using Sales.Services.API.Features.Shared;
using RFQEntity = Welco.Shared.Domain.Models.RFQ;
namespace Sales.Services.API.Features.RFQs.Queries.GetRFQById
{
    public class GetRFQByIdQueryHandler : IRequestHandler<GetRFQByIdQuery, Result<RFQDto>>
    {
        private readonly IUnitOfWork _uow; private readonly ICurrentUserService _cur;
        public GetRFQByIdQueryHandler(IUnitOfWork uow, ICurrentUserService cur) { _uow = uow; _cur = cur; }
        public async Task<Result<RFQDto>> Handle(GetRFQByIdQuery r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<RFQEntity, Guid>();
            var rfq = await repo.GetAll(x => !x.IsDeleted && x.Id == r.Id).Include(x => x.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(ct);
            if (rfq == null) return Result<RFQDto>.NotFound(LocalizationKeys.RFQ.NotFound);
            var caller = await BuyerScope.GetAsync(_uow, _cur, ct);
            if (caller.IsOrganizationUser && caller.CompanyId != rfq.CompanyId) return Result<RFQDto>.NotFound(LocalizationKeys.RFQ.NotFound);
            return Result<RFQDto>.Success(new RFQDto { Id = rfq.Id, RFQNumber = rfq.RFQNumber, CompanyId = rfq.CompanyId, Status = rfq.Status.ToString(), AssignedSalesRepId = rfq.AssignedSalesRepId, Items = rfq.Items.Where(i => !i.IsDeleted).Select(i => new RFQItemDto { Id = i.Id, RFQId = i.RFQId, ProductId = i.ProductId, ProductNameEn = i.Product != null ? i.Product.NameEn : null, ProductNameAr = i.Product != null ? i.Product.NameAr : null, ImageName = i.Product != null ? i.Product.ImageName : null, Quantity = i.Quantity, UnitPrice = i.UnitPrice, Notes = i.Notes }).ToList(), CreatedAt = rfq.CreatedAt }, LocalizationKeys.RFQ.Fetched);
        }
    }
}
