namespace Welco.Shared.Common.Options
{
    public class ExchangeRateSettings
    {
        public const string SectionName = "ExchangeRateSettings";

        public string Provider { get; set; } = "YahooFinance";
        public string BaseCurrency { get; set; } = "USD";
        public string BaseUrl { get; set; } = "https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies";
        public int TimeoutSeconds { get; set; } = 10;
        /// <summary>Safety margin % added on conversion totals (not display rates). 0 disables.</summary>
        public decimal SafetyMarginPercent { get; set; } = 0.5m;
    }
}
