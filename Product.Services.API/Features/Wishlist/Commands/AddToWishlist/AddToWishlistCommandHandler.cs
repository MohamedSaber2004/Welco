using MediatR;
using Microsoft.EntityFrameworkCore;
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

            // Ensure the user exists in database to prevent FK constraint violations
            var userRepo = _unitOfWork.GetRepository<ApplicationUser, Guid>();
            var existsUser = await userRepo.ExistsAsync(u => !u.IsDeleted && u.Id == userId, cancellationToken);
            if (!existsUser)
                return Result<string>.Unauthorized(LocalizationKeys.ExceptionMessages.Unauthorized);

            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var existsProduct = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Id == request.ProductId, cancellationToken);
            if (!existsProduct)
                return Result<string>.NotFound(LocalizationKeys.Product.NotFound);

            var wishlistRepo = _unitOfWork.GetRepository<UserProductInteraction, Guid>();

            // Query including soft-deleted items to prevent duplicate key constraint violations on IX_UserProductInteractions_UserId_ProductId_Type
            var existing = await wishlistRepo.GetAll()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == request.ProductId && w.Type == "Wishlist", cancellationToken);

            if (existing != null)
            {
                if (existing.IsDeleted)
                {
                    existing.Active();
                    existing.Timestamp = DateTime.UtcNow;
                    existing.MarkAsUpdated(userId.ToString());
                    wishlistRepo.Update(existing);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                return Result<string>.Success(existing.Id.ToString(), LocalizationKeys.Product.AddedToWishlist);
            }

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

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Concurrency safe fallback: if already inserted by a parallel request, return success
                return Result<string>.Success(interaction.Id.ToString(), LocalizationKeys.Product.AddedToWishlist);
            }

            return Result<string>.Created(interaction.Id.ToString(), LocalizationKeys.Product.AddedToWishlist);
        }
    }
}
