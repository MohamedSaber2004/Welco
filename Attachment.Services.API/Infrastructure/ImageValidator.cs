using Microsoft.Extensions.Localization;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Localization;

namespace Attachment.Services.API.Infrastructure
{
    public class ImageValidator : IImageValidator
    {
        private readonly IBaseFileService _baseFileService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<Messages> _localizer;

        public ImageValidator(IBaseFileService baseFileService, IHttpClientFactory httpClientFactory, IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadImage(IFormFile file, int Place)
        {
            if (!IsValidImage(file))
                return (false, _localizer["Attachments:InvalidFormat"]);

            if (file.Length > FilePathHelper.MaxImageSize)
                return (false, _localizer["Attachments:FileTooLarge"]);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, FilePathHelper.GetFolderPath(Place));
            if (uploaded)
            {
                return (true, FilePathHelper.ToStoredName(Place, result));
            }
            return (false, result);
        }

        public async Task<(bool Uploaded, string Result)> UploadMultipleImage(List<IFormFile> files, int Place)
        {
            if (files == null || !files.Any())
                return (false, _localizer["Attachments:NoMediaProvided"]);

            var results = new List<string>();
            foreach (var file in files)
            {
                var (uploaded, result) = await UploadImage(file, Place);
                if (uploaded)
                {
                    results.Add(result);
                }
            }

            if (!results.Any())
                return (false, _localizer["Attachments:UploadFailed"]);

            return (true, string.Join(",", results));
        }

        public bool ImageIsExisted(string? FullImagePath)
        {
            return _baseFileService.FileExists(FullImagePath);
        }

        public async Task<bool> DeleteImage(string fileName, int Place)
        {
            var (place, cleanFileName) = FilePathHelper.ParsePlace(fileName, Place);
            return await _baseFileService.DeleteFileAsync(cleanFileName, FilePathHelper.GetFolderPath(place));
        }

        public string GetUniqueFileName(string fileName)
        {
            return _baseFileService.GetUniqueFileName(fileName);
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        public bool IsValidImage(string ImageName, string PlaceHolder)
        {
            return !string.IsNullOrEmpty(ImageName) && ImageName != PlaceHolder;
        }

        public async Task<IFormFile?> ConvertImageToFormFile(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(imageUrl);
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsByteArrayAsync();
                var stream = new MemoryStream(content);

                var fileName = Path.GetFileName(imageUrl);
                if (string.IsNullOrEmpty(fileName))
                    fileName = "downloaded_image.jpg";

                return new FormFile(stream, 0, content.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream"
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
