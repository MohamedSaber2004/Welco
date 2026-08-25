using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Welco.Shared.Enums;
using Welco.Shared.Localization.Interfaces;

namespace Welco.Shared.Localization
{
    public class JsonLocalizationProvider : ILocalizationProvider
    {
        private readonly ILogger<JsonLocalizationProvider> _logger;
        private readonly Dictionary<string, Dictionary<string, string>> _localizations = new(StringComparer.OrdinalIgnoreCase);

        public JsonLocalizationProvider(ILogger<JsonLocalizationProvider> logger, string? resourcesPath = null)
        {
            _logger = logger;
            LoadResources(resourcesPath);
        }

        private void LoadResources(string? resourcesPath)
        {
            var assembly = typeof(JsonLocalizationProvider).Assembly;

            // 1. Load from Embedded Resources (Always guaranteed to be available in-memory)
            foreach (var langCode in AppLanguageExtensions.GetAllCodes())
            {
                var resourceName = $"Welco.Shared.Localization.Resources.messages.{langCode}.json";
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(stream);
                        var cultureData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        FlattenJson(doc.RootElement, "", cultureData);
                        _localizations[langCode] = cultureData;
                        _logger.LogInformation("Successfully loaded {Count} keys from embedded resource for language: {Language}", cultureData.Count, langCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing embedded localization resource {ResourceName}", resourceName);
                    }
                }
            }

            // 2. Also check physical disk files if present
            var baseDirectory = AppContext.BaseDirectory;
            var assemblyLocation = Path.GetDirectoryName(assembly.Location);

            var possiblePaths = new List<string>();
            if (!string.IsNullOrEmpty(resourcesPath)) possiblePaths.Add(resourcesPath);
            possiblePaths.Add(Path.Combine(baseDirectory, "Localization", "Resources"));
            possiblePaths.Add(Path.Combine(assemblyLocation ?? "", "Localization", "Resources"));
            possiblePaths.Add(Path.Combine(Directory.GetCurrentDirectory(), "Localization", "Resources"));

            var resourcePath = possiblePaths.FirstOrDefault(Directory.Exists);
            if (resourcePath != null)
            {
                foreach (var langCode in AppLanguageExtensions.GetAllCodes())
                {
                    var filePath = Path.Combine(resourcePath, $"messages.{langCode}.json");
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            var json = File.ReadAllText(filePath);
                            using var doc = JsonDocument.Parse(json);
                            var cultureData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            FlattenJson(doc.RootElement, "", cultureData);
                            _localizations[langCode] = cultureData;
                            _logger.LogInformation("Successfully loaded {Count} keys from file {FilePath}", cultureData.Count, filePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error loading localization file {FilePath}", filePath);
                        }
                    }
                }
            }
        }

        private static void FlattenJson(JsonElement element, string prefix, Dictionary<string, string> result)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var name = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                        FlattenJson(property.Value, name, result);
                    }
                    break;
                case JsonValueKind.Array:
                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        FlattenJson(item, $"{prefix}[{index}]", result);
                        index++;
                    }
                    break;
                case JsonValueKind.String:
                    result[prefix] = element.GetString() ?? "";
                    break;
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    result[prefix] = element.ToString();
                    break;
            }
        }

        public string GetLocalizedString(string key, string? culture = null)
        {
            culture ??= CultureInfo.CurrentUICulture?.Name;
            var language = AppLanguageExtensions.FromCode(culture);
            var normalizedCode = language.ToCode();

            if (_localizations.TryGetValue(normalizedCode, out var cultureData) && cultureData.TryGetValue(key, out var value))
            {
                return value;
            }

            // Fallback to English
            var englishCode = AppLanguage.En.ToCode();
            if (normalizedCode != englishCode && _localizations.TryGetValue(englishCode, out var enData) && enData.TryGetValue(key, out var enValue))
            {
                return enValue;
            }

            return key;
        }

        public string GetLocalizedString(string key, string? culture, params object[] args)
        {
            var baseValue = GetLocalizedString(key, culture);

            if (args == null || args.Length == 0)
            {
                return baseValue;
            }

            try
            {
                return string.Format(baseValue, args);
            }
            catch
            {
                return baseValue;
            }
        }
    }
}