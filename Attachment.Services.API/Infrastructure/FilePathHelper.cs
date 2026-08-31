using Welco.Shared.Common.Services;

namespace Attachment.Services.API.Infrastructure
{
    internal static class FilePathHelper
    {
        public static (int Place, string FileName) ParsePlace(string fileName, int defaultPlace)
        {
            if (!string.IsNullOrWhiteSpace(fileName) && fileName.Contains('_'))
            {
                var parts = fileName.Split('_');
                if (parts.Length > 1 && int.TryParse(parts[0], out var place))
                {
                    return (place, string.Join("_", parts.Skip(1)));
                }
            }

            return (defaultPlace, fileName ?? string.Empty);
        }

        public static string GetFolderPath(int place)
        {
            return UploadPaths.GetPath(place) ?? "Uploads";
        }

        public static string ToStoredName(int place, string result)
        {
            return $"{place}_{Path.GetFileName(result)}";
        }
    }
}
