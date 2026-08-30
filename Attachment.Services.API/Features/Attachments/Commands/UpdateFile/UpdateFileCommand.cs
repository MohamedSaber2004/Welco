using MediatR;
using Welco.Shared.Enums;

namespace Attachment.Services.API.Features.Attachments.Commands.UpdateFile
{
    public class UpdateFileCommand : IRequest<string>
    {
        public IFormFile File { get; set; } = null!;
        public string? OldFileName { get; set; }
        public int Place { get; set; }
        public MediaType FileType { get; set; }
    }
}
