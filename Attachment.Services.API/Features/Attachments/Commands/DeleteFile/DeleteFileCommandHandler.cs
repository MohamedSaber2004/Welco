using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Enums;

namespace Attachment.Services.API.Features.Attachments.Commands.DeleteFile
{
    public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, bool>
    {
        private readonly IImageValidator _imageValidator;
        private readonly IAudioValidator _audioValidator;
        private readonly IVideoValidator _videoValidator;
        private readonly IFileValidator _fileValidator;

        public DeleteFileCommandHandler(
            IImageValidator imageValidator,
            IAudioValidator audioValidator,
            IVideoValidator videoValidator,
            IFileValidator fileValidator)
        {
            _imageValidator = imageValidator;
            _audioValidator = audioValidator;
            _videoValidator = videoValidator;
            _fileValidator = fileValidator;
        }

        public async Task<bool> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FileName))
                return false;

            return request.FileType switch
            {
                MediaType.Image => await _imageValidator.DeleteImage(request.FileName, request.Place),
                MediaType.Audio => await _audioValidator.DeleteAudio(request.FileName, request.Place),
                MediaType.Video => await _videoValidator.DeleteVideo(request.FileName, request.Place),
                MediaType.File => await _fileValidator.DeleteFile(request.FileName, request.Place),
                _ => await _imageValidator.DeleteImage(request.FileName, request.Place)
            };
        }
    }
}
