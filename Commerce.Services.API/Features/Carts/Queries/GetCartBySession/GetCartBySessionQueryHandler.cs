using Commerce.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CartEntity = Welco.Shared.Domain.Models.Cart;

namespace Commerce.Services.API.Features.Carts.Queries.GetCartBySession
{
    public class GetCartBySessionQueryHandler : IRequestHandler<GetCartBySessionQuery, Result<CartDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetCartBySessionQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<CartDto>> Handle(GetCartBySessionQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<CartEntity, Guid>();
            var cart = await repo.GetAll(c => !c.IsDeleted && c.SessionId == request.SessionId)
                .Include(c => c.Items)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
                return Result<CartDto>.NotFound(LocalizationKeys.Cart.NotFound);

            var dto = CommerceDtoMapper.ToDto(cart);
            return Result<CartDto>.Success(dto, LocalizationKeys.Cart.Fetched);
        }
    }
}
