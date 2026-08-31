using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using DocEntity = Welco.Shared.Domain.Models.Document;

namespace Content.Services.API.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, Result<DocumentDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetDocumentByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Result<DocumentDto>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<DocEntity, Guid>();
            var dto = await repo.GetAll(d => !d.IsDeleted && d.Id == request.Id)
                .Select(ContentDtoMapper.DocumentProjection)
                .FirstOrDefaultAsync(cancellationToken);

            if (dto == null)
                return Result<DocumentDto>.NotFound(LocalizationKeys.Document.NotFound);

            return Result<DocumentDto>.Success(dto, LocalizationKeys.Document.Fetched);
        }
    }
}
