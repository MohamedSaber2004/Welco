using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Products.Queries.GetProductVideos
{
    public class GetProductVideosQueryHandler : IRequestHandler<GetProductVideosQuery, Result<IReadOnlyList<ProductMediaDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductVideosQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<ProductMediaDto>>> Handle(GetProductVideosQuery request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var productExists = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Id == request.ProductId, cancellationToken);
            if (!productExists)
                return Result<IReadOnlyList<ProductMediaDto>>.NotFound(LocalizationKeys.Product.NotFound);

            var mediaRepo = _unitOfWork.GetRepository<ProductMedia, Guid>();
            var videos = await mediaRepo.GetAll(m => !m.IsDeleted && m.ProductId == request.ProductId && m.Type == ProductMediaType.Video)
                .OrderBy(m => m.SortOrder)
                .Select(m => new ProductMediaDto
                {
                    Id = m.Id,
                    ProductId = m.ProductId,
                    Type = (int)m.Type,
                    Url = m.Url,
                    SortOrder = m.SortOrder
                })
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<ProductMediaDto>>.Success(videos, LocalizationKeys.Product.Fetched);
        }
    }
}
