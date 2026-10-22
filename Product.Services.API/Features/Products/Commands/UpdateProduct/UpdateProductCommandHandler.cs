using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Services.API.Common;
using Product.Services.API.Features.Shared;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CategoryEntity = Welco.Shared.Domain.Models.Category;
using CurrencyEntity = Welco.Shared.Domain.Models.Currency;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProductCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var product = await productRepo.GetByIdAsync(request.Id, cancellationToken);

            if (product == null || product.IsDeleted)
                return Result<ProductDto>.NotFound(LocalizationKeys.Product.NotFound);

            var scope = await ProviderScope.GetAsync(_unitOfWork, _currentUserService, cancellationToken);
            if (scope.IsOrganizationUser &&
                (!scope.CompanyId.HasValue || product.CompanyId != scope.CompanyId.Value))
                return Result<ProductDto>.NotFound(LocalizationKeys.Product.NotFound);

            var scopeId = scope.IsOrganizationUser ? scope.CompanyId : product.CompanyId;

            if (!string.Equals(product.Sku, request.Sku.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var skuExists = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Id != request.Id && p.CompanyId == scopeId && p.Sku.ToLower() == request.Sku.Trim().ToLower(), cancellationToken);
                if (skuExists) return Result<ProductDto>.Conflict(LocalizationKeys.Product.SkuAlreadyExists);
            }
            if (!string.Equals(product.Slug, request.Slug.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var slugExists = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Id != request.Id && p.Slug.ToLower() == request.Slug.Trim().ToLower(), cancellationToken);
                if (slugExists) return Result<ProductDto>.Conflict(LocalizationKeys.Product.SlugAlreadyExists);
            }

            var categoryRepo = _unitOfWork.GetRepository<CategoryEntity, Guid>();
            var categoryExists = await categoryRepo.ExistsAsync(c => !c.IsDeleted && c.Id == request.CategoryId, cancellationToken);
            if (!categoryExists)
                return Result<ProductDto>.BadRequest(LocalizationKeys.Product.CategoryNotFound);

            if (request.CurrencyId.HasValue)
            {
                var currencyRepo = _unitOfWork.GetRepository<CurrencyEntity, Guid>();
                var currencyExists = await currencyRepo.ExistsAsync(c => !c.IsDeleted && c.Id == request.CurrencyId.Value, cancellationToken);
                if (!currencyExists)
                    return Result<ProductDto>.BadRequest(LocalizationKeys.Currency.NotFound);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty ? _currentUserService.UserId.ToString() : "System";

            var nameAr = string.IsNullOrWhiteSpace(request.NameAr) ? (product.NameAr ?? request.NameEn.Trim()) : request.NameAr.Trim();
            var price = request.Price ?? product.Price;
            var stock = request.Stock ?? product.Stock;
            var primaryImage = (request.Images != null && request.Images.Count > 0)
                ? request.Images[0].Trim()
                : (string.IsNullOrWhiteSpace(request.ImageName) ? product.ImageName : request.ImageName.Trim());

            product.Update(
                request.NameEn.Trim(),
                nameAr,
                request.Sku.Trim(),
                request.Slug.Trim().ToLowerInvariant(),
                request.Description,
                price,
                stock,
                request.Specifications,
                primaryImage,
                request.Material,
                request.LengthCm,
                request.CurrencyId,
                request.CategoryId,
                currentUserId,
                request.IsMostSelling ?? product.IsMostSelling);

            if (request.IsActive.HasValue)
                product.SetActiveState(request.IsActive.Value, currentUserId);

            productRepo.Update(product);

            var mediaRepo = _unitOfWork.GetRepository<ProductMedia, Guid>();

            if (request.Images != null)
            {
                var oldImages = await mediaRepo.GetAll(m => !m.IsDeleted && m.ProductId == product.Id && m.Type == ProductMediaType.Image)
                    .ToListAsync(cancellationToken);
                foreach (var old in oldImages)
                {
                    old.MarkAsDeleted(currentUserId);
                    mediaRepo.Update(old);
                }

                int order = 0;
                foreach (var img in request.Images)
                {
                    if (string.IsNullOrWhiteSpace(img)) continue;
                    order++;
                    var media = new ProductMedia
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Type = ProductMediaType.Image,
                        Url = img.Trim(),
                        SortOrder = order
                    };
                    media.MarkAsCreated(currentUserId);
                    await mediaRepo.AddAsync(media, cancellationToken);
                }
            }

            if (request.VideoUrl != null)
            {
                var oldVideos = await mediaRepo.GetAll(m => !m.IsDeleted && m.ProductId == product.Id && m.Type == ProductMediaType.Video)
                    .ToListAsync(cancellationToken);
                foreach (var old in oldVideos)
                {
                    old.MarkAsDeleted(currentUserId);
                    mediaRepo.Update(old);
                }

                if (!string.IsNullOrWhiteSpace(request.VideoUrl))
                {
                    var videoMedia = new ProductMedia
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Type = ProductMediaType.Video,
                        Url = request.VideoUrl.Trim(),
                        SortOrder = 1
                    };
                    videoMedia.MarkAsCreated(currentUserId);
                    await mediaRepo.AddAsync(videoMedia, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = await productRepo.GetAll(p => !p.IsDeleted && p.Id == product.Id)
                .Select(ProductDtoMapper.Projection)
                .FirstOrDefaultAsync(cancellationToken);

            if (dto != null)
            {
                if (request.Images != null && request.Images.Count > 0)
                {
                    dto.Images = request.Images.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                }
                else if (!string.IsNullOrWhiteSpace(dto.ImageName))
                {
                    dto.Images = new List<string> { dto.ImageName };
                }
                dto.VideoUrl = request.VideoUrl;
            }

            return Result<ProductDto>.Success(dto!, LocalizationKeys.Product.Updated);
        }
    }
}
