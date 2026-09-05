using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Products.Commands.UpdateProductVideos
{
    public class UpdateProductVideosCommandHandler : IRequestHandler<UpdateProductVideosCommand, Result<IReadOnlyList<ProductMediaDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProductVideosCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<IReadOnlyList<ProductMediaDto>>> Handle(UpdateProductVideosCommand request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.GetRepository<ProductEntity, Guid>();
            var exists = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Id == request.ProductId, cancellationToken);
            if (!exists)
                return Result<IReadOnlyList<ProductMediaDto>>.NotFound(LocalizationKeys.Product.NotFound);

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            var mediaRepo = _unitOfWork.GetRepository<ProductMedia, Guid>();

            // Replace the whole video set: soft-delete current videos, then insert the new list.
            var current = await mediaRepo.GetAll(m => !m.IsDeleted && m.ProductId == request.ProductId && m.Type == ProductMediaType.Video)
                .ToListAsync(cancellationToken);
            foreach (var m in current)
            {
                m.MarkAsDeleted(currentUserId);
                mediaRepo.Update(m);
            }

            var order = 0;
            foreach (var v in request.Videos ?? new List<UpdateProductVideoItemDto>())
            {
                if (string.IsNullOrWhiteSpace(v.Url)) continue;
                order++;
                var media = new ProductMedia
                {
                    Id = Guid.NewGuid(),
                    ProductId = request.ProductId,
                    Type = ProductMediaType.Video,
                    Url = v.Url.Trim(),
                    SortOrder = v.SortOrder > 0 ? v.SortOrder : order
                };
                media.MarkAsCreated(currentUserId);
                await mediaRepo.AddAsync(media, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

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

            return Result<IReadOnlyList<ProductMediaDto>>.Success(videos, LocalizationKeys.Product.Updated);
        }
    }
}
