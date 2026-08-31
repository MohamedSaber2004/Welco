using MediatR;
using Welco.Shared.Results;
using Welco.Shared.Common.DTOs.Content;
namespace Content.Services.API.Features.Documents.Queries.GetDocuments
{
    public class GetDocumentsQuery : IRequest<PaginatedResult<DocumentDto>> { public int PageNumber { get; set; } = 1; public int PageSize { get; set; } = 10; }
}
