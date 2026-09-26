using MediatR;
using Welco.Shared.Enums;

namespace Attachment.Services.API.Features.Attachments.Commands.DeleteFile
{
    public class DeleteFileCommand : IRequest<bool>
    {
        public string FileName { get; set; } = string.Empty;
        public int Place { get; set; } = 1;
        public MediaType FileType { get; set; } = MediaType.Image;
    }
}
