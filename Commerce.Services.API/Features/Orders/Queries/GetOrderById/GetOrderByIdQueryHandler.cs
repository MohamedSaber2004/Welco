using Commerce.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using OrderEntity = Welco.Shared.Domain.Models.Order;

namespace Commerce.Services.API.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetOrderByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<OrderEntity, Guid>();
            var order = await repo.GetAll(o => !o.IsDeleted && o.Id == request.Id)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
                return Result<OrderDto>.NotFound(LocalizationKeys.Order.NotFound);

            var dto = CommerceDtoMapper.ToDto(order);
            return Result<OrderDto>.Success(dto, LocalizationKeys.Order.Fetched);
        }
    }
}
