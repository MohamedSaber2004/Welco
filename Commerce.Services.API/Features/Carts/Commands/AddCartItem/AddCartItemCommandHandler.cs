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
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Commerce.Services.API.Features.Carts.Commands.AddCartItem
{
    public class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, Result<CartDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public AddCartItemCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<CartDto>> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
        {
            var cartRepo = _uow.GetRepository<CartEntity, Guid>();
            var cart = await cartRepo.GetAll(c => !c.IsDeleted && c.Id == request.CartId)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
                return Result<CartDto>.NotFound(LocalizationKeys.Cart.NotFound);

            var productRepo = _uow.GetRepository<ProductEntity, Guid>();
            var productExists = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Id == request.ProductId, cancellationToken);
            if (!productExists)
                return Result<CartDto>.NotFound(LocalizationKeys.Product.NotFound);

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";

            var cartItemRepo = _uow.GetRepository<CartItemEntity, Guid>();
            var existing = cart.Items.FirstOrDefault(i => !i.IsDeleted && i.ProductId == request.ProductId);

            if (existing != null)
            {
                existing.Quantity += request.Quantity;
                existing.UnitPriceSnapshot = request.UnitPriceSnapshot;
                existing.MarkAsUpdated(currentUserId);
                cartItemRepo.Update(existing);
            }
            else
            {
                var item = new CartItemEntity
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPriceSnapshot = request.UnitPriceSnapshot
                };
                item.MarkAsCreated(currentUserId);
                await cartItemRepo.AddAsync(item, cancellationToken);
            }

            await _uow.SaveChangesAsync(cancellationToken);

            var refreshed = await cartRepo.GetAll(c => !c.IsDeleted && c.Id == cart.Id)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(cancellationToken);

            var dto = CommerceDtoMapper.ToDto(refreshed!);
            return Result<CartDto>.Success(dto, LocalizationKeys.Cart.ItemAdded);
        }
    }
}
