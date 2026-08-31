using MediatR;
using Product.Services.API.Common;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Products.Queries.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedResult<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var query = productRepo.GetAll(p => !p.IsDeleted);

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
                query = query.Where(p => p.Sku.ToLower() == request.Sku.Trim().ToLower());

            if (!string.IsNullOrWhiteSpace(request.Material))
                query = query.Where(p => p.Material != null && p.Material.ToLower() == request.Material.Trim().ToLower());

            if (request.LengthMin.HasValue)
                query = query.Where(p => p.LengthCm.HasValue && p.LengthCm.Value >= request.LengthMin.Value);

            if (request.LengthMax.HasValue)
                query = query.Where(p => p.LengthCm.HasValue && p.LengthCm.Value <= request.LengthMax.Value);

            if (request.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == request.CategoryId.Value);

            if (request.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == request.IsActive.Value);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToPaginatedListAsync(
                    ProductDtoMapper.Projection,
                    request.PageNumber,
                    request.PageSize,
                    LocalizationKeys.Product.ListFetched,
                    cancellationToken);
        }
    }
}
