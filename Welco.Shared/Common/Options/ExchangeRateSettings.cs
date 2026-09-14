namespace Welco.Shared.Common.Options
{
    public class ExchangeRateSettings
    {
        public const string SectionName = "ExchangeRateSettings";

    public string Provider { get; set; } = "FastForex";
    public string BaseCurrency { get; set; } = "USD";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.fastforex.io";
        public int SyncIntervalHours { get; set; } = 24;
        public int TimeoutSeconds { get; set; } = 10;
        public int CacheExpirationMinutes { get; set; } = 60;
    }
}
