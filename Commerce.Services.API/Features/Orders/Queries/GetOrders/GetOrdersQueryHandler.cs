using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using OrderEntity = Welco.Shared.Domain.Models.Order;

namespace Commerce.Services.API.Features.Orders.Queries.GetOrders
{
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PaginatedResult<OrderDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetOrdersQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PaginatedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<OrderEntity, Guid>();
            var query = repo.GetAll(o => !o.IsDeleted).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<Welco.Shared.Domain.Models.OrderStatus>(request.Status, true, out var st))
                query = query.Where(o => o.Status == st);

            if (request.UserId.HasValue)
                query = query.Where(o => o.UserId == request.UserId.Value);

            if (request.CompanyId.HasValue)
                query = query.Where(o => o.CompanyId == request.CompanyId.Value);

            return await query.OrderByDescending(o => o.CreatedAt)
                .ToPaginatedListAsync(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status.ToString(),
                    UserId = o.UserId,
                    CompanyId = o.CompanyId,
                    CurrencyId = o.CurrencyId,
                    TotalAmount = o.TotalAmount,
                    IsActive = o.IsActive,
                    CreatedAt = o.CreatedAt,
                    Items = new List<OrderItemDto>()
                }, request.PageNumber, request.PageSize, LocalizationKeys.Order.ListFetched, cancellationToken);
        }
    }
}
