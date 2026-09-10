namespace Welco.Shared.Common.Options
{
        public class WelcoServiceSettings
    {
        public const string SectionName = "WelcoServiceSettings";

                public string ServiceSecret { get; set; } = string.Empty;

                public string ServiceIssuer { get; set; } = "snul-integration";

                public string ServiceAudience { get; set; } = "welco-integration";

                public int TokenExpiryMinutes { get; set; } = 60;
    }
}
