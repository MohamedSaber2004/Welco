using Microsoft.Extensions.Localization;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Localization;

namespace Attachment.Services.API.Infrastructure
{
    public class VideoValidator : IVideoValidator
    {
        private const long MaxVideoSizeBytes = 100 * 1024 * 1024;
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".avi", ".mkv", ".mov", ".wmv" };

        private readonly IBaseFileService _baseFileService;
        private readonly IStringLocalizer<Messages> _localizer;

        public VideoValidator(IBaseFileService baseFileService, IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadVideo(IFormFile file, int Place)
        {
            if (file == null || file.Length == 0)
                return (false, _localizer[LocalizationKeys.AttachmentMessages.FileEmpty]);

            if (!IsValidVideo(file))
                return (false, _localizer[LocalizationKeys.AttachmentMessages.InvalidFormat]);

            if (file.Length > MaxVideoSizeBytes)
                return (false, _localizer[LocalizationKeys.AttachmentMessages.FileTooLarge]);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, FilePathHelper.GetFolderPath(Place));
            if (uploaded)
            {
                return (true, FilePathHelper.ToStoredName(Place, result));
            }
            return (false, result);
        }

        public async Task<(bool Uploaded, string Result)> UploadMultipleVideo(List<IFormFile> files, int Place)
        {
            if (files == null || !files.Any())
                return (false, _localizer[LocalizationKeys.AttachmentMessages.NoMediaProvided]);

            var results = new List<string>();
            foreach (var file in files)
            {
                var (uploaded, result) = await UploadVideo(file, Place);
                if (uploaded)
                {
                    results.Add(result);
                }
            }

            if (!results.Any())
                return (false, _localizer[LocalizationKeys.AttachmentMessages.UploadFailed]);

            return (true, string.Join(",", results));
        }

        public bool VideoIsExisted(string? FullVideoPath)
        {
            return _baseFileService.FileExists(FullVideoPath);
        }

        public async Task<bool> DeleteVideo(string fileName, int Place)
        {
            var (place, cleanFileName) = FilePathHelper.ParsePlace(fileName, Place);
            return await _baseFileService.DeleteFileAsync(cleanFileName, FilePathHelper.GetFolderPath(place));
        }

        public string GetUniqueFileName(string fileName)
        {
            return _baseFileService.GetUniqueFileName(fileName);
        }

        public bool IsValidVideo(string VideoName, string PlaceHolder)
        {
            return !string.IsNullOrEmpty(VideoName) && VideoName != PlaceHolder;
        }

        public bool IsValidVideo(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;
            if (file.Length > MaxVideoSizeBytes) return false;

            var extension = Path.GetExtension(file.FileName).ToLower();
            return AllowedVideoExtensions.Contains(extension);
        }
    }
}
