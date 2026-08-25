namespace Welco.Shared.Enums
{
    public enum AppLanguage
    {
        En = 1,
        Ar = 2
    }

    public static class AppLanguageExtensions
    {
        public const string EnglishCode = "en";
        public const string ArabicCode = "ar";

        public static string ToCode(this AppLanguage language) => language switch
        {
            AppLanguage.Ar => ArabicCode,
            AppLanguage.En => EnglishCode,
            _ => EnglishCode
        };

        public static AppLanguage FromCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return AppLanguage.En;
            }

            // Support comma-separated Accept-Language headers (e.g. "ar-EG,ar;q=0.9,en-US;q=0.8" or "en-US,en;q=0.9")
            var parts = code.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                var langPart = part.Split(';')[0].Trim().ToLowerInvariant();

                if (langPart.StartsWith("ar"))
                {
                    return AppLanguage.Ar;
                }

                if (langPart.StartsWith("en"))
                {
                    return AppLanguage.En;
                }
            }

            return AppLanguage.En;
        }

        public static string[] GetAllCodes() => new[]
        {
            AppLanguage.En.ToCode(),
            AppLanguage.Ar.ToCode()
        };
    }
}
