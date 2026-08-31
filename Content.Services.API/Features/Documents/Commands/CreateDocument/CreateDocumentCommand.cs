using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommand : IRequest<Result<DocumentDto>>
    {
        public string Title { get; set; } = string.Empty;
        public string DocType { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public int FileSizeKB { get; set; }
        public Guid? ProductId { get; set; }
        public DateTime PublishedDate { get; set; }
    }
}
