using MediatR;
using Welco.Shared.Common.DTOs.Attachments;

namespace Attachment.Services.API.Features.Attachments.Commands.DownloadFile
{
    public class DownloadFileCommand : IRequest<FileResponseDto>
    {
        public int Place { get; set; }
        public string FileName { get; set; } = null!;
    }
}
