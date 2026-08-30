using Microsoft.Extensions.Localization;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Localization;

namespace Attachment.Services.API.Infrastructure
{
    public class AudioValidator : IAudioValidator
    {
        private readonly IBaseFileService _baseFileService;
        private readonly IStringLocalizer<Messages> _localizer;

        public AudioValidator(IBaseFileService baseFileService, IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadAudio(IFormFile file, int Place)
        {
            if (!IsValidAudio(file))
                return (false, _localizer["Attachments:InvalidFormat"]);

            if (file.Length > FilePathHelper.MaxAudioSize)
                return (false, _localizer["Attachments:FileTooLarge"]);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, FilePathHelper.GetFolderPath(Place));
            if (uploaded)
            {
                return (true, FilePathHelper.ToStoredName(Place, result));
            }
            return (false, result);
        }

        public bool AudioIsExisted(string? FullAudioPath)
        {
            return _baseFileService.FileExists(FullAudioPath);
        }

        public async Task<bool> DeleteAudio(string fileName, int Place)
        {
            var (place, cleanFileName) = FilePathHelper.ParsePlace(fileName, Place);
            return await _baseFileService.DeleteFileAsync(cleanFileName, FilePathHelper.GetFolderPath(place));
        }

        public string GetUniqueFileName(string fileName)
        {
            return _baseFileService.GetUniqueFileName(fileName);
        }

        public bool IsValidAudio(string AudioName, string PlaceHolder)
        {
            return !string.IsNullOrEmpty(AudioName) && AudioName != PlaceHolder;
        }

        public bool IsValidAudio(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            var allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".aac" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }
    }
}
