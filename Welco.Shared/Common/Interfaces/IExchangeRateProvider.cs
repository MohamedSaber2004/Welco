namespace Welco.Shared.Common.Interfaces
{
    public class ExchangeRateResponse
    {
        public string BaseCurrency { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string Source { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    }

    public interface IExchangeRateProvider
    {
        string ProviderName { get; }

        /// <param name="targetCodes">Currency codes to quote, from the database.
        /// Providers whose API returns full tables may ignore it.</param>
        Task<ExchangeRateResponse> GetLatestRatesAsync(
            string baseCurrency,
            IReadOnlyCollection<string>? targetCodes,
            CancellationToken cancellationToken);

        Task<ExchangeRateResponse?> GetHistoricalRatesAsync(
            string baseCurrency,
            DateOnly date,
            IReadOnlyCollection<string>? targetCodes,
            CancellationToken cancellationToken);
    }
}
