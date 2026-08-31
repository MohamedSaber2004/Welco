using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQuery : IRequest<Result<DocumentDto>>
    {
        public Guid Id { get; set; }
    }
}
