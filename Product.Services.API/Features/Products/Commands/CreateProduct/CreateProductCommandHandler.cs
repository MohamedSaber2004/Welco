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

namespace Product.Services.API.Features.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();

            var scope = await ProviderScope.GetAsync(_unitOfWork, _currentUserService, cancellationToken);
            Guid? companyId = null;
            if (scope.IsOrganizationUser)
            {
                if (!scope.CompanyId.HasValue)
                    return Result<ProductDto>.Forbidden();
                companyId = scope.CompanyId.Value;
            }

            var sku = request.Sku.Trim();
            var skuExists = await productRepo.ExistsAsync(p => !p.IsDeleted && p.CompanyId == companyId && p.Sku.ToLower() == sku.ToLower(), cancellationToken);
            if (skuExists)
                return Result<ProductDto>.Conflict(LocalizationKeys.Product.SkuAlreadyExists);

            var slug = request.Slug.Trim().ToLowerInvariant();
            var slugExists = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Slug.ToLower() == slug, cancellationToken);
            if (slugExists)
                return Result<ProductDto>.Conflict(LocalizationKeys.Product.SlugAlreadyExists);

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

            var nameAr = string.IsNullOrWhiteSpace(request.NameAr) ? request.NameEn.Trim() : request.NameAr.Trim();
            var price = request.Price ?? 0m;
            var stock = request.Stock ?? 0;
            var primaryImage = (request.Images != null && request.Images.Count > 0)
                ? request.Images[0].Trim()
                : (string.IsNullOrWhiteSpace(request.ImageName) ? null : request.ImageName.Trim());

            var product = ProductEntity.Create(
                request.NameEn.Trim(),
                nameAr,
                sku,
                slug,
                request.Description,
                price,
                stock,
                request.Specifications,
                primaryImage,
                request.Material,
                request.LengthCm,
                request.CurrencyId,
                request.CategoryId,
                companyId,
                currentUserId,
                request.IsMostSelling);

            await productRepo.AddAsync(product, cancellationToken);

            var mediaRepo = _unitOfWork.GetRepository<ProductMedia, Guid>();

            if (request.Images != null && request.Images.Count > 0)
            {
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

            return Result<ProductDto>.Created(dto!, LocalizationKeys.Product.Created);
        }
    }
}
