using Microsoft.Extensions.Configuration;
using Welco.Shared.Common.Options;

namespace Welco.Shared.Common.Services
{
    public class UploadPaths
    {
        private static UploadPathsOptions? Options;

        public static void Configure(IConfiguration configuration)
        {
            Options = configuration.GetSection("UploadPaths").Get<UploadPathsOptions>();
        }

        public static string? DefaultPath => Options?.DefaultPath;

        public static string? GetPath(int place)
        {
            return place switch
            {
                0 => DefaultPath,
                1 => Options?.Providers,
                2 => Options?.Users,
                _ => null
            };
        }

        public static IEnumerable<string> GetAllPaths()
        {
            if (Options is null) yield break;

            foreach (var path in new[] { Options.Providers, Options.Users })
            {
                if (!string.IsNullOrWhiteSpace(path))
                    yield return path;
            }
        }

        public static string? GetStorageRoot()
        {
            if (Options is null || string.IsNullOrWhiteSpace(Options.RootPath))
                return null;

            return Path.GetFullPath(Options.RootPath, AppContext.BaseDirectory);
        }
    }
}
