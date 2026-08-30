using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace Welco.Shared.Common.Services
{
    public class CustomFileProvider : IFileProvider
    {
        private readonly string _wwwRootPath;

        public CustomFileProvider(string? wwwRootPath = null)
        {
            _wwwRootPath = !string.IsNullOrWhiteSpace(wwwRootPath)
                ? Path.GetFullPath(wwwRootPath)
                : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var fileNameWithPrefix = Path.GetFileName(subpath) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fileNameWithPrefix))
            {
                return new NotFoundFileInfo(subpath);
            }

            var (place, actualFileName) = SplitPlacePrefix(fileNameWithPrefix);

            if (place.HasValue && !string.IsNullOrWhiteSpace(actualFileName))
            {
                var folderPath = UploadPaths.GetPath(place.Value);
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    var fileLocation = GetSafeFileLocation(folderPath, actualFileName);
                    if (fileLocation != null && File.Exists(fileLocation))
                        return new PhysicalFileInfo(new FileInfo(fileLocation));
                }
            }

            foreach (var path in UploadPaths.GetAllPaths())
            {
                if (string.IsNullOrWhiteSpace(path)) continue;

                var fileLocation = GetSafeFileLocation(path, fileNameWithPrefix);
                if (fileLocation != null && File.Exists(fileLocation))
                    return new PhysicalFileInfo(new FileInfo(fileLocation));
            }

            var rootFileLocation = GetSafeFileLocation(null, fileNameWithPrefix);
            if (rootFileLocation != null && File.Exists(rootFileLocation))
                return new PhysicalFileInfo(new FileInfo(rootFileLocation));

            return new NotFoundFileInfo(fileNameWithPrefix);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return NotFoundDirectoryContents.Singleton;
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }

        private static (int? Place, string? FileName) SplitPlacePrefix(string fileName)
        {
            if (fileName.Contains('_'))
            {
                var parts = fileName.Split('_');
                if (parts.Length > 1 && int.TryParse(parts[0], out var place))
                {
                    return (place, string.Join("_", parts.Skip(1)));
                }
            }

            return (null, null);
        }

        private string? GetSafeFileLocation(string? folderPath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            var safeFileName = Path.GetFileName(fileName);
            var fullPath = string.IsNullOrWhiteSpace(folderPath)
                ? Path.Combine(_wwwRootPath, safeFileName)
                : Path.Combine(_wwwRootPath, folderPath, safeFileName);

            var fullPathRoot = Path.GetFullPath(fullPath);
            if (!fullPathRoot.StartsWith(_wwwRootPath, StringComparison.OrdinalIgnoreCase))
                return null;

            return fullPathRoot;
        }
    }
}
