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

    public class ConversionResult
    {
        public decimal Amount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal ConvertedAmount { get; set; }
        public DateOnly RateDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public int DecimalDigits { get; set; } = 2;
    }

    public interface IExchangeRateProvider
    {
        string ProviderName { get; }

        /// <summary>
        /// Live daily rates via FastForex fetch-one
        /// (GET fetch-one?from={FROM}&amp;to={TO}&amp;api_key={KEY} per pair).
        /// </summary>
        /// <param name="targetCodes">Currency codes to quote.</param>
        Task<ExchangeRateResponse> GetLatestRatesAsync(
            string baseCurrency,
            IReadOnlyCollection<string>? targetCodes,
            CancellationToken cancellationToken);

        /// <summary>Single-pair conversion via fetch-one; dated from "updated" (daily).</summary>
        Task<ConversionResult> ConvertAsync(
            string fromCurrency,
            string toCurrency,
            decimal amount,
            CancellationToken cancellationToken);
    }
}
