using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using RFQEntity = Welco.Shared.Domain.Models.RFQ;
namespace Sales.Services.API.Features.RFQs.Queries.GetRFQById
{
    public class GetRFQByIdQueryHandler : IRequestHandler<GetRFQByIdQuery, Result<RFQDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetRFQByIdQueryHandler(IUnitOfWork uow) => _uow = uow;
        public async Task<Result<RFQDto>> Handle(GetRFQByIdQuery r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<RFQEntity, Guid>();
            var rfq = await repo.GetAll(x => !x.IsDeleted && x.Id == r.Id).Include(x => x.Items).FirstOrDefaultAsync(ct);
            if (rfq == null) return Result<RFQDto>.NotFound(LocalizationKeys.RFQ.NotFound);
            return Result<RFQDto>.Success(new RFQDto { Id = rfq.Id, RFQNumber = rfq.RFQNumber, CompanyId = rfq.CompanyId, Status = rfq.Status.ToString(), AssignedSalesRepId = rfq.AssignedSalesRepId, Items = rfq.Items.Where(i => !i.IsDeleted).Select(i => new RFQItemDto { Id = i.Id, RFQId = i.RFQId, ProductId = i.ProductId, Quantity = i.Quantity, Notes = i.Notes }).ToList(), CreatedAt = rfq.CreatedAt }, LocalizationKeys.RFQ.Fetched);
        }
    }
}
