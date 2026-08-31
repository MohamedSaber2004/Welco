using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using DocEntity = Welco.Shared.Domain.Models.Document;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Content.Services.API.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, Result<DocumentDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public CreateDocumentCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<DocumentDto>> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
        {
            if (request.ProductId.HasValue)
            {
                var productRepo = _uow.GetRepository<ProductEntity, Guid>();
                var exists = await productRepo.ExistsAsync(p => !p.IsDeleted && p.Id == request.ProductId.Value, cancellationToken);
                if (!exists)
                    return Result<DocumentDto>.NotFound(LocalizationKeys.Product.NotFound);
            }

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";

            var doc = new DocEntity
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                DocType = request.DocType.Trim(),
                FileUrl = request.FileUrl.Trim(),
                FileSizeKB = request.FileSizeKB,
                ProductId = request.ProductId,
                PublishedDate = request.PublishedDate == default ? DateTime.UtcNow : request.PublishedDate
            };
            doc.MarkAsCreated(currentUserId);

            var repo = _uow.GetRepository<DocEntity, Guid>();
            await repo.AddAsync(doc, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = await repo.GetAll(d => !d.IsDeleted && d.Id == doc.Id)
                .Select(ContentDtoMapper.DocumentProjection)
                .FirstOrDefaultAsync(cancellationToken);

            return Result<DocumentDto>.Created(dto!, LocalizationKeys.Document.Created);
        }
    }
}
