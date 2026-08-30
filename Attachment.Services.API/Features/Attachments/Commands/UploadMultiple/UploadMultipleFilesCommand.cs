using MediatR;

namespace Attachment.Services.API.Features.Attachments.Commands.UploadMultiple
{
    public class UploadMultipleFilesCommand : IRequest<List<string>>
    {
        public List<IFormFile>? Images { get; set; }
        public int ImagesPlace { get; set; }

        public List<IFormFile>? Videos { get; set; }
        public int VideosPlace { get; set; }

        public List<IFormFile>? Audios { get; set; }
        public int AudiosPlace { get; set; }

        public List<IFormFile>? Documents { get; set; }
        public int DocumentsPlace { get; set; }
    }
}
