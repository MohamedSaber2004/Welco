using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Domain.Models;

namespace Welco.Shared.Common.Interfaces
{
    public interface IExchangeRateService
    {
        Task<ExchangeRateDto?> GetLatestRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken);
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken);
        Task<ConversionResultDto> ConvertWithDetailsAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<ExchangeRateDto>> GetLatestRatesAsync(string baseCurrency, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<ExchangeRateDto>> GetHistoricalRatesAsync(string baseCurrency, DateOnly date, CancellationToken cancellationToken);
        Task<ExchangeRateSyncResult> SyncLatestRatesAsync(CancellationToken cancellationToken);
        Task<ExchangeRateSyncResult> SyncHistoricalRatesAsync(DateOnly date, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<ExchangeRateSyncLog>> GetSyncLogsAsync(int take, CancellationToken cancellationToken);
    }

    public class ExchangeRateSyncResult
    {
        public bool Success { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public int RatesCount { get; set; }
        public DateOnly RateDate { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
