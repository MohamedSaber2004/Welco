using Commerce.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using OrderEntity = Welco.Shared.Domain.Models.Order;

namespace Commerce.Services.API.Features.Orders.Queries.TrackOrder
{
    public class TrackOrderQueryHandler : IRequestHandler<TrackOrderQuery, Result<OrderDto>>
    {
        private readonly IUnitOfWork _uow;
        public TrackOrderQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<OrderDto>> Handle(TrackOrderQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.OrderNumber))
                return Result<OrderDto>.Failure(LocalizationKeys.Order.OrderNumberRequired);

            var repo = _uow.GetRepository<OrderEntity, Guid>();
            var trimmed = request.OrderNumber.Trim().ToLower();

            var order = await repo.GetAll(o => !o.IsDeleted && o.OrderNumber.ToLower() == trimmed)
                .Include(o => o.Currency)
                .Include(o => o.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
                return Result<OrderDto>.NotFound(LocalizationKeys.Order.NotFound);

            var dto = CommerceDtoMapper.ToDto(order);
            return Result<OrderDto>.Success(dto, LocalizationKeys.Order.Fetched);
        }
    }
}
