using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using RFQEntity = Welco.Shared.Domain.Models.RFQ;
namespace Sales.Services.API.Features.RFQs.Queries.GetRFQs
{
    public class GetRFQsQueryHandler : IRequestHandler<GetRFQsQuery, PaginatedResult<RFQDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetRFQsQueryHandler(IUnitOfWork uow) => _uow = uow;
        public async Task<PaginatedResult<RFQDto>> Handle(GetRFQsQuery r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<RFQEntity, Guid>();
            var q = repo.GetAll(x => !x.IsDeleted).AsNoTracking();
            return await q.OrderByDescending(x => x.CreatedAt).ToPaginatedListAsync(x => new RFQDto { Id = x.Id, RFQNumber = x.RFQNumber, CompanyId = x.CompanyId, Status = x.Status.ToString(), AssignedSalesRepId = x.AssignedSalesRepId, CreatedAt = x.CreatedAt }, r.PageNumber, r.PageSize, LocalizationKeys.RFQ.ListFetched, ct);
        }
    }
}
