using Commerce.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CartEntity = Welco.Shared.Domain.Models.Cart;
using CartItemEntity = Welco.Shared.Domain.Models.CartItem;

namespace Commerce.Services.API.Features.Carts.Commands.RemoveCartItem
{
    public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, Result<CartDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public RemoveCartItemCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<CartDto>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
        {
            var cartRepo = _uow.GetRepository<CartEntity, Guid>();
            var cart = await cartRepo.GetAll(c => !c.IsDeleted && c.Id == request.CartId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
                return Result<CartDto>.NotFound(LocalizationKeys.Cart.NotFound);

            var itemRepo = _uow.GetRepository<CartItemEntity, Guid>();
            var item = await itemRepo.GetAll(i => !i.IsDeleted && i.Id == request.ItemId && i.CartId == request.CartId)
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null)
                return Result<CartDto>.NotFound(LocalizationKeys.Cart.ItemNotFound);

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            item.MarkAsDeleted(currentUserId);
            itemRepo.Update(item);
            await _uow.SaveChangesAsync(cancellationToken);

            var refreshed = await cartRepo.GetAll(c => !c.IsDeleted && c.Id == cart.Id)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(cancellationToken);

            var dto = CommerceDtoMapper.ToDto(refreshed!);
            return Result<CartDto>.Success(dto, LocalizationKeys.Cart.ItemRemoved);
        }
    }
}
