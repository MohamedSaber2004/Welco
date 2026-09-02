using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Wishlist.Commands.AddToWishlist
{
    public class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public AddToWishlistCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == Guid.Empty)
                return Result<string>.Unauthorized(LocalizationKeys.ExceptionMessages.Unauthorized);

            var userId = _currentUserService.UserId;
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var existsProduct = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Id == request.ProductId, cancellationToken);
            if (!existsProduct)
                return Result<string>.NotFound(LocalizationKeys.Product.NotFound);

            var wishlistRepo = _unitOfWork.GetRepository<UserProductInteraction, Guid>();
            var already = await wishlistRepo.ExistsAsync(w => !w.IsDeleted && w.UserId == userId && w.ProductId == request.ProductId && w.Type == "Wishlist", cancellationToken);
            if (already)
                return Result<string>.Success(request.ProductId.ToString(), LocalizationKeys.Product.Fetched); // idempotent

            var interaction = new UserProductInteraction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = request.ProductId,
                Type = "Wishlist",
                Timestamp = DateTime.UtcNow
            };
            interaction.MarkAsCreated(userId.ToString());
            await wishlistRepo.AddAsync(interaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<string>.Created(interaction.Id.ToString(), "Added to favourites");
        }
    }
}
