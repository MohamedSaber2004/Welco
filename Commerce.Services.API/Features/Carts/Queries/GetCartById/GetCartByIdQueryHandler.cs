using Commerce.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Commerce;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CartEntity = Welco.Shared.Domain.Models.Cart;

namespace Commerce.Services.API.Features.Carts.Queries.GetCartById
{
    public class GetCartByIdQueryHandler : IRequestHandler<GetCartByIdQuery, Result<CartDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetCartByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<CartDto>> Handle(GetCartByIdQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<CartEntity, Guid>();
            var cart = await repo.GetAll(c => !c.IsDeleted && c.Id == request.Id)
                .Include(c => c.Items)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
                return Result<CartDto>.NotFound(LocalizationKeys.Cart.NotFound);

            // Load Product names for items if needed
            // Use mapper ToDto that handles IsDeleted filter
            var dto = CommerceDtoMapper.ToDto(cart);
            return Result<CartDto>.Success(dto, LocalizationKeys.Cart.Fetched);
        }
    }
}
