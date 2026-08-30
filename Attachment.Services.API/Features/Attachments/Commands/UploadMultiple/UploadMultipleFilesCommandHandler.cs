using MediatR;
using Welco.Shared.Common.Interfaces;

namespace Attachment.Services.API.Features.Attachments.Commands.UploadMultiple
{
    public class UploadMultipleFilesCommandHandler : IRequestHandler<UploadMultipleFilesCommand, List<string>>
    {
        private readonly IImageValidator _imageValidator;
        private readonly IVideoValidator _videoValidator;
        private readonly IFileValidator _fileValidator;
        private readonly IAudioValidator _audioValidator;

        public UploadMultipleFilesCommandHandler(
            IImageValidator imageValidator,
            IVideoValidator videoValidator,
            IFileValidator fileValidator,
            IAudioValidator audioValidator)
        {
            _imageValidator = imageValidator;
            _videoValidator = videoValidator;
            _fileValidator = fileValidator;
            _audioValidator = audioValidator;
        }

        public async Task<List<string>> Handle(UploadMultipleFilesCommand request, CancellationToken cancellationToken)
        {
            var results = new List<string>();

            // Process Images
            if (request.Images != null && request.Images.Any())
            {
                var (uploaded, result) = await _imageValidator.UploadMultipleImage(request.Images, request.ImagesPlace);
                if (uploaded) results.AddRange(result.Split(','));
            }

            // Process Videos
            if (request.Videos != null && request.Videos.Any())
            {
                var (uploaded, result) = await _videoValidator.UploadMultipleVideo(request.Videos, request.VideosPlace);
                if (uploaded) results.AddRange(result.Split(','));
            }

            // Process Audios
            if (request.Audios != null && request.Audios.Any())
            {
                foreach (var audio in request.Audios)
                {
                    var (uploaded, result) = await _audioValidator.UploadAudio(audio, request.AudiosPlace);
                    if (uploaded) results.Add(result);
                }
            }

            // Process Documents
            if (request.Documents != null && request.Documents.Any())
            {
                var (uploaded, result) = await _fileValidator.UploadMultipleFile(request.Documents, request.DocumentsPlace);
                if (uploaded) results.AddRange(result.Split(','));
            }

            return results.Where(r => !string.IsNullOrEmpty(r)).ToList();
        }
    }
}
