namespace Welco.Shared.Common.Options
{
    public class IntegrationClient
    {
        public string ClientId { get; set; } = string.Empty;

        public string Secret { get; set; } = string.Empty;

        public string Market { get; set; } = "Egypt";
    }

    public class WelcoServiceSettings
    {
        public const string SectionName = "WelcoServiceSettings";

        public string ServiceSecret { get; set; } = string.Empty;

        public string ServiceIssuer { get; set; } = "snul-integration";

        public string ServiceAudience { get; set; } = "welco-integration";

        public int TokenExpiryMinutes { get; set; } = 60;

        public Dictionary<string, IntegrationClient> Clients { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
