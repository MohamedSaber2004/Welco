using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CartEntity = Welco.Shared.Domain.Models.Cart;
using CartItemEntity = Welco.Shared.Domain.Models.CartItem;

namespace Commerce.Services.API.Features.Carts.Commands.ClearCart
{
    public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public ClearCartCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<string>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
        {
            var cartRepo = _uow.GetRepository<CartEntity, Guid>();
            var cart = await cartRepo.GetAll(c => !c.IsDeleted && c.Id == request.CartId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
                return Result<string>.NotFound(LocalizationKeys.Cart.NotFound);

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            var itemRepo = _uow.GetRepository<CartItemEntity, Guid>();
            var activeItems = cart.Items.Where(i => !i.IsDeleted).ToList();

            foreach (var item in activeItems)
            {
                item.MarkAsDeleted(currentUserId);
                itemRepo.Update(item);
            }

            if (activeItems.Any())
                await _uow.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(cart.Id.ToString(), LocalizationKeys.Cart.Cleared);
        }
    }
}
