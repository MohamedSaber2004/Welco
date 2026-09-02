using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Wishlist.Queries.IsInWishlist
{
    public class IsInWishlistQueryHandler : IRequestHandler<IsInWishlistQuery, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public IsInWishlistQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<bool>> Handle(IsInWishlistQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == Guid.Empty)
                return Result<bool>.Success(false, LocalizationKeys.Product.Fetched);

            var userId = _currentUserService.UserId;
            var repo = _unitOfWork.GetRepository<UserProductInteraction, Guid>();
            var exists = await repo.ExistsAsync(w => !w.IsDeleted && w.UserId == userId && w.ProductId == request.ProductId && w.Type == "Wishlist", cancellationToken);
            return Result<bool>.Success(exists, LocalizationKeys.Product.Fetched);
        }
    }
}
