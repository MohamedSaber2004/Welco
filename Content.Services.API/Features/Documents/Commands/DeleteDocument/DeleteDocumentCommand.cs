using MediatR;
using Welco.Shared.Results;

namespace Content.Services.API.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
