using Commerce.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using OrderEntity = Welco.Shared.Domain.Models.Order;

namespace Commerce.Services.API.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public GetOrderByIdQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<OrderEntity, Guid>();
            var order = await repo.GetAll(o => !o.IsDeleted && o.Id == request.Id)
                .Include(o => o.Currency)
                .Include(o => o.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
                return Result<OrderDto>.NotFound(LocalizationKeys.Order.NotFound);

            if (_currentUser.UserId != Guid.Empty)
            {
                var userRepo = _uow.GetRepository<ApplicationUser, Guid>();
                var user = await userRepo.GetByIdAsync(_currentUser.UserId, cancellationToken);
                if (user != null && !user.IsDeleted && user.UserType == UserType.OrganizationUser)
                {
                    var isOwner = order.UserId == user.Id || (user.CompanyId.HasValue && order.CompanyId == user.CompanyId.Value);
                    if (!isOwner) return Result<OrderDto>.NotFound(LocalizationKeys.Order.NotFound);
                }
            }

            var dto = CommerceDtoMapper.ToDto(order);
            return Result<OrderDto>.Success(dto, LocalizationKeys.Order.Fetched);
        }
    }
}
