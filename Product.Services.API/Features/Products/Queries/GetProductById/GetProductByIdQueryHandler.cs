using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Services.API.Common;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var product = await productRepo.GetAll(p => !p.IsDeleted && p.Id == request.Id)
                .Select(ProductDtoMapper.Projection)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                return Result<ProductDto>.NotFound(LocalizationKeys.Product.NotFound);
            }

            var mediaRepo = _unitOfWork.GetRepository<ProductMedia, Guid>();
            var mediaList = await mediaRepo.GetAll(m => !m.IsDeleted && m.ProductId == request.Id)
                .OrderBy(m => m.SortOrder)
                .ToListAsync(cancellationToken);

            product.Images = mediaList
                .Where(m => m.Type == ProductMediaType.Image && !string.IsNullOrWhiteSpace(m.Url))
                .Select(m => m.Url)
                .ToList();

            if (product.Images.Count == 0 && !string.IsNullOrWhiteSpace(product.ImageName))
            {
                product.Images.Add(product.ImageName);
            }

            product.VideoUrl = mediaList
                .FirstOrDefault(m => m.Type == ProductMediaType.Video)?.Url;

            return Result<ProductDto>.Success(product, LocalizationKeys.Product.Fetched);
        }
    }
}
