using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Services.API.Common;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Products.Queries.ShowProduct
{
    public class ShowProductQueryHandler : IRequestHandler<ShowProductQuery, Result<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShowProductQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ProductDto>> Handle(ShowProductQuery request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var product = await productRepo.GetAll(p => !p.IsDeleted && p.Id == request.Id)
                .Select(ProductDtoMapper.Projection)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                return Result<ProductDto>.NotFound(LocalizationKeys.Product.NotFound);
            }

            return Result<ProductDto>.Success(product, LocalizationKeys.Product.Fetched);
        }
    }
}
