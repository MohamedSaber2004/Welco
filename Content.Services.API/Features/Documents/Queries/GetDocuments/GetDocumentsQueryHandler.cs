using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using DocEntity = Welco.Shared.Domain.Models.Document;
namespace Content.Services.API.Features.Documents.Queries.GetDocuments
{
    public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, PaginatedResult<DocumentDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetDocumentsQueryHandler(IUnitOfWork uow) => _uow = uow;
        public async Task<PaginatedResult<DocumentDto>> Handle(GetDocumentsQuery r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<DocEntity, Guid>();
            var q = repo.GetAll(x => !x.IsDeleted).AsNoTracking();
            return await q.OrderByDescending(x => x.CreatedAt).ToPaginatedListAsync(x => new DocumentDto { Id = x.Id, Title = x.Title, DocType = x.DocType, FileUrl = x.FileUrl, FileSizeKB = x.FileSizeKB, ProductId = x.ProductId, PublishedDate = x.PublishedDate, CreatedAt = x.CreatedAt }, r.PageNumber, r.PageSize, LocalizationKeys.Document.ListFetched, ct);
        }
    }
}
