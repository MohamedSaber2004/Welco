using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Services.API.Common;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CategoryEntity = Welco.Shared.Domain.Models.Category;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Categories.Queries.GetCategoryProducts
{
    public class GetCategoryProductsQueryHandler : IRequestHandler<GetCategoryProductsQuery, Result<List<ProductDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoryProductsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<ProductDto>>> Handle(GetCategoryProductsQuery request, CancellationToken cancellationToken)
        {
            var categoryRepo = _unitOfWork.GetRepository<CategoryEntity, Guid>();
            var categoryExists = await categoryRepo.ExistsAsync(c => !c.IsDeleted && c.Id == request.CategoryId, cancellationToken);
            if (!categoryExists)
            {
                return Result<List<ProductDto>>.NotFound(LocalizationKeys.Category.NotFound);
            }

            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var products = await productRepo.GetAll(p => !p.IsDeleted && p.CategoryId == request.CategoryId)
                .OrderBy(p => p.NameEn)
                .Select(ProductDtoMapper.Projection)
                .ToListAsync(cancellationToken);

            return Result<List<ProductDto>>.Success(products, LocalizationKeys.Category.ProductsFetched);
        }
    }
}
