using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Services;
using Welco.Shared.Localization;

namespace Attachment.Services.API.Infrastructure
{
    public class BaseFileService : IBaseFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly ILogger<BaseFileService> _logger;

        private string WebRootPath => UploadPaths.GetStorageRoot()
            ?? _webHostEnvironment.WebRootPath
            ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");

        public BaseFileService(
            IWebHostEnvironment webHostEnvironment,
            IStringLocalizer<Messages> localizer,
            ILogger<BaseFileService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<(bool Uploaded, string Result)> UploadFileAsync(IFormFile file, string folderPath)
        {
            if (file == null || file.Length == 0)
                return (false, _localizer[LocalizationKeys.AttachmentMessages.FileEmpty]);

            try
            {
                string uploadsFolder = Path.Combine(WebRootPath, folderPath);

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = GetUniqueFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(fileStream);
                }

                string relativePath = Path.Combine(folderPath, uniqueFileName).Replace("\\", "/");
                _logger.LogInformation("Uploaded file '{FileName}' to '{RelativePath}'", uniqueFileName, relativePath);

                return (true, relativePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file '{FileName}' to folder '{FolderPath}'", file.FileName, folderPath);
                return (false, _localizer[LocalizationKeys.AttachmentMessages.UploadFailed]);
            }
        }

        public bool FileExists(string? fullFilePath)
        {
            if (string.IsNullOrWhiteSpace(fullFilePath))
                return false;

            var filePath = GetSafeFilePath(fullFilePath.TrimStart('/'));
            return filePath != null && File.Exists(filePath);
        }

        public async Task<bool> DeleteFileAsync(string fileName, string folderPath)
        {
            try
            {
                var filePath = GetSafeFilePath(Path.Combine(folderPath, fileName));
                if (filePath == null)
                    return false;

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted file '{FilePath}'", filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file '{FileName}' from folder '{FolderPath}'", fileName, folderPath);
                return false;
            }
        }

        public string GetUniqueFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return Guid.NewGuid().ToString("N") + extension;
        }

        public Task<(bool Success, string Result)> DownloadFileAsync(string folderPath, string fileName)
        {
            var relativePath = Path.Combine(folderPath, fileName).Replace("\\", "/");

            if (FileExists(relativePath))
            {
                return Task.FromResult((true, relativePath));
            }

            return Task.FromResult((false, _localizer[LocalizationKeys.AttachmentMessages.FileNotFound].Value));
        }

        private string? GetSafeFilePath(string relativePath)
        {
            var webRootPath = Path.GetFullPath(WebRootPath);
            var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath.TrimStart('/', '\\')));

            if (!fullPath.StartsWith(webRootPath, StringComparison.OrdinalIgnoreCase))
                return null;

            return fullPath;
        }
    }
}
