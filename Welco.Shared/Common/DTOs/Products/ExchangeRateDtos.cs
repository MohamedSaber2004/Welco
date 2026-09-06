namespace Welco.Shared.Common.DTOs.Products
{
    public class ExchangeRateDto
    {
        public Guid Id { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public string TargetCurrency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateOnly RateDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; }
    }

    public class ConversionResultDto
    {
        public decimal Amount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public decimal ConvertedAmount { get; set; }
        public DateOnly RateDate { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public class ExchangeRateSyncLogDto
    {
        public Guid Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BaseCurrency { get; set; } = string.Empty;
        public int RatesCount { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
