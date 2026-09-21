using MediatR;
using Product.Services.API.Common;
using Product.Services.API.Features.Shared;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Products.Queries.GetMyProducts
{
    public class GetMyProductsQueryHandler : IRequestHandler<GetMyProductsQuery, PaginatedResult<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetMyProductsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PaginatedResult<ProductDto>> Handle(GetMyProductsQuery request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var scope = await ProviderScope.GetAsync(_unitOfWork, _currentUserService, cancellationToken);

            // No linked company (or not an org user acting as provider):
            // return an empty page rather than leaking other catalogs.
            if (!scope.IsOrganizationUser || !scope.CompanyId.HasValue)
            {
                return PaginatedResult<ProductDto>.Success(
                    new List<ProductDto>(),
                    0,
                    request.PageNumber <= 0 ? 1 : request.PageNumber,
                    request.PageSize <= 0 ? 10 : request.PageSize,
                    LocalizationKeys.Product.ListFetched);
            }

            var companyId = scope.CompanyId.Value;
            var query = productRepo.GetAll(p => !p.IsDeleted && p.CompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.NameEn.ToLower().Contains(term) ||
                    p.NameAr.ToLower().Contains(term) ||
                    p.Sku.ToLower().Contains(term) ||
                    (p.Material != null && p.Material.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(request.Sku))
                query = query.Where(p => p.Sku.ToLower().Contains(request.Sku.Trim().ToLower()));

            if (request.CategoryId.HasValue)
            {
                var targetCatId = request.CategoryId.Value;
                query = query.Where(p => p.CategoryId == targetCatId || (p.Category != null && p.Category.ParentCategoryId == targetCatId));
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            return await query
                .ToPaginatedListAsync(
                    ProductDtoMapper.Projection,
                    request.PageNumber,
                    request.PageSize,
                    LocalizationKeys.Product.ListFetched,
                    cancellationToken);
        }
    }
}
