using Microsoft.Extensions.Localization;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Localization;

namespace Attachment.Services.API.Infrastructure
{
    public class FileValidator : IFileValidator
    {
        private readonly IBaseFileService _baseFileService;
        private readonly IStringLocalizer<Messages> _localizer;

        public FileValidator(IBaseFileService baseFileService, IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadFile(IFormFile file, int Place)
        {
            if (!IsValidFile(file))
                return (false, _localizer["Attachments:InvalidFormat"]);

            if (file.Length > FilePathHelper.DefaultMaxFileSize)
                return (false, _localizer["Attachments:FileTooLarge"]);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, FilePathHelper.GetFolderPath(Place));
            if (uploaded)
            {
                return (true, FilePathHelper.ToStoredName(Place, result));
            }
            return (false, result);
        }

        public bool FileIsExisted(string? FullFilePath)
        {
            return _baseFileService.FileExists(FullFilePath);
        }

        public async Task<bool> DeleteFile(string fileName, int Place)
        {
            var (place, cleanFileName) = FilePathHelper.ParsePlace(fileName, Place);
            return await _baseFileService.DeleteFileAsync(cleanFileName, FilePathHelper.GetFolderPath(place));
        }

        public string GetUniqueFileName(string fileName)
        {
            return _baseFileService.GetUniqueFileName(fileName);
        }

        public bool IsValidFile(string FileName, string PlaceHolder)
        {
            return !string.IsNullOrEmpty(FileName) && FileName != PlaceHolder;
        }

        public bool IsValidFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip", ".rar" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        public async Task<(bool Success, string Result)> DownloadFile(int FilePlace, string FileName)
        {
            var (place, cleanFileName) = FilePathHelper.ParsePlace(FileName, FilePlace);
            return await _baseFileService.DownloadFileAsync(FilePathHelper.GetFolderPath(place), cleanFileName);
        }

        public async Task<(bool Uploaded, string Result)> UploadMultipleFile(List<IFormFile> files, int Place)
        {
            if (files == null || !files.Any())
                return (false, _localizer["Attachments:NoMediaProvided"]);

            var results = new List<string>();
            foreach (var file in files)
            {
                var (uploaded, result) = await UploadFile(file, Place);
                if (uploaded)
                {
                    results.Add(result);
                }
            }

            if (!results.Any())
                return (false, _localizer["Attachments:UploadFailed"]);

            return (true, string.Join(",", results));
        }
    }
}
