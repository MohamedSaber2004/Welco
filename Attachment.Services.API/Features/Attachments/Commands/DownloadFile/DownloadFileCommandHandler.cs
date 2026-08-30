using MediatR;
using Microsoft.AspNetCore.StaticFiles;
using Welco.Shared.Common.DTOs.Attachments;
using Welco.Shared.Common.Interfaces;

namespace Attachment.Services.API.Features.Attachments.Commands.DownloadFile
{
    public class DownloadFileCommandHandler : IRequestHandler<DownloadFileCommand, FileResponseDto>
    {
        private readonly IFileValidator _fileValidator;

        public DownloadFileCommandHandler(IFileValidator fileValidator)
        {
            _fileValidator = fileValidator;
        }

        public async Task<FileResponseDto> Handle(DownloadFileCommand request, CancellationToken cancellationToken)
        {
            var (success, result) = await _fileValidator.DownloadFile(request.Place, request.FileName);

            var contentType = "application/octet-stream";
            if (success)
            {
                var provider = new FileExtensionContentTypeProvider();
                if (provider.TryGetContentType(result, out var detectedType))
                {
                    contentType = detectedType;
                }
            }

            return new FileResponseDto
            {
                Success = success,
                FilePath = success ? result : string.Empty,
                FileName = success ? Path.GetFileName(result) : string.Empty,
                ContentType = contentType,
                ErrorMessage = success ? null : result
            };
        }
    }
}
