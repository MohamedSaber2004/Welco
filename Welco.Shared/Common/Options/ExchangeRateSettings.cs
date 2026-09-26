namespace Welco.Shared.Common.Options
{
    public class ExchangeRateSettings
    {
        public const string SectionName = "ExchangeRateSettings";

        public string Provider { get; set; } = "FawazahmedCDN";
        public string BaseCurrency { get; set; } = "USD";
        public string BaseUrl { get; set; } = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies";
        public int TimeoutSeconds { get; set; } = 10;
    }
}
