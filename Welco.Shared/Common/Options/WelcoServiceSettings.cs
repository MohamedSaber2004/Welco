namespace Welco.Shared.Common.Options
{
    public class WelcoServiceSettings
    {
        public const string SectionName = "WelcoServiceSettings";

        public string ServiceSecret { get; set; } = string.Empty;

        public int TokenExpiryMinutes { get; set; } = 60;
    }
}
