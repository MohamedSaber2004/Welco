namespace Welco.Shared.Common.Options
{
    public class ExchangeRateSettings
    {
        public const string SectionName = "ExchangeRateSettings";

        public string Provider { get; set; } = "FawazahmedCDN";
        public string BaseCurrency { get; set; } = "USD";
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies";
        public int SyncIntervalHours { get; set; } = 24;
        public int TimeoutSeconds { get; set; } = 10;
        public int CacheExpirationMinutes { get; set; } = 60;
    }
}
