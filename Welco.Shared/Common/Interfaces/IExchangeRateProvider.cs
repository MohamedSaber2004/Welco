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

        Task<ExchangeRateResponse> GetLatestRatesAsync(
            string baseCurrency,
            CancellationToken cancellationToken);

        Task<ExchangeRateResponse?> GetHistoricalRatesAsync(
            string baseCurrency,
            DateOnly date,
            CancellationToken cancellationToken);
    }
}
