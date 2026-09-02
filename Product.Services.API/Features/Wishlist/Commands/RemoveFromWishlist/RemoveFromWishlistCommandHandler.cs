using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Wishlist.Commands.RemoveFromWishlist
{
    public class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public RemoveFromWishlistCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == Guid.Empty)
                return Result<string>.Unauthorized(LocalizationKeys.ExceptionMessages.Unauthorized);

            var userId = _currentUserService.UserId;
            var repo = _unitOfWork.GetRepository<UserProductInteraction, Guid>();
            var interaction = await repo.GetFirstAsync(w => !w.IsDeleted && w.UserId == userId && w.ProductId == request.ProductId && w.Type == "Wishlist", cancellationToken);
            if (interaction == null)
                return Result<string>.NotFound(LocalizationKeys.Product.NotFound);

            interaction.MarkAsDeleted(userId.ToString());
            repo.Update(interaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<string>.Success(request.ProductId.ToString(), "Removed from favourites");
        }
    }
}
