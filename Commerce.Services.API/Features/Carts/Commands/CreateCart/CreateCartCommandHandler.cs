using Commerce.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CartEntity = Welco.Shared.Domain.Models.Cart;
using CurrencyEntity = Welco.Shared.Domain.Models.Currency;

namespace Commerce.Services.API.Features.Carts.Commands.CreateCart
{
    public class CreateCartCommandHandler : IRequestHandler<CreateCartCommand, Result<CartDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public CreateCartCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<CartDto>> Handle(CreateCartCommand request, CancellationToken cancellationToken)
        {
            if (!request.UserId.HasValue && string.IsNullOrWhiteSpace(request.SessionId))
                return Result<CartDto>.BadRequest(LocalizationKeys.Cart.UserIdOrSessionRequired);

            if (request.CurrencyId.HasValue)
            {
                var currencyRepo = _uow.GetRepository<CurrencyEntity, Guid>();
                var exists = await currencyRepo.ExistsAsync(c => !c.IsDeleted && c.Id == request.CurrencyId.Value, cancellationToken);
                if (!exists)
                    return Result<CartDto>.BadRequest(LocalizationKeys.Currency.NotFound);
            }

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";

            var cart = new CartEntity
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                SessionId = request.SessionId?.Trim(),
                CurrencyId = request.CurrencyId
            };
            cart.MarkAsCreated(currentUserId);

            var repo = _uow.GetRepository<CartEntity, Guid>();
            await repo.AddAsync(cart, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var saved = await repo.GetAll(c => !c.IsDeleted && c.Id == cart.Id)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(cancellationToken);

            var dto = CommerceDtoMapper.ToDto(saved!);
            return Result<CartDto>.Created(dto, LocalizationKeys.Cart.Created);
        }
    }
}
