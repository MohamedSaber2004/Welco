using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Services.API.Common;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Wishlist.Queries.GetWishlist
{
    public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, Result<IReadOnlyList<ProductDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetWishlistQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<IReadOnlyList<ProductDto>>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated || _currentUserService.UserId == Guid.Empty)
                return Result<IReadOnlyList<ProductDto>>.Unauthorized(LocalizationKeys.ExceptionMessages.Unauthorized);

            var userId = _currentUserService.UserId;
            var wishlistRepo = _unitOfWork.GetRepository<UserProductInteraction, Guid>();
            var productIds = await wishlistRepo.GetAll(w => !w.IsDeleted && w.UserId == userId && w.Type == "Wishlist")
                .Select(w => w.ProductId)
                .ToListAsync(cancellationToken);

            if (!productIds.Any())
                return Result<IReadOnlyList<ProductDto>>.Success(Array.Empty<ProductDto>(), LocalizationKeys.Product.Fetched);

            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var products = await productRepo.GetAll(p => !p.IsDeleted && productIds.Contains(p.Id))
                .Select(ProductDtoMapper.Projection)
                .ToListAsync(cancellationToken);

            // Preserve wishlist order (most recent first via Timestamp)
            var ordered = await wishlistRepo.GetAll(w => !w.IsDeleted && w.UserId == userId && w.Type == "Wishlist")
                .OrderByDescending(w => w.Timestamp)
                .Select(w => w.ProductId)
                .ToListAsync(cancellationToken);
            var dict = products.ToDictionary(p => p.Id);
            var sorted = ordered.Where(id => dict.ContainsKey(id)).Select(id => dict[id]).ToList();

            return Result<IReadOnlyList<ProductDto>>.Success(sorted, LocalizationKeys.Product.Fetched);
        }
    }
}
