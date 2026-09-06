using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public enum ExchangeRateSyncStatus
    {
        Success = 1,
        Failed = 2,
        Partial = 3
    }

    public class ExchangeRateSyncLog : BaseEntity<Guid>
    {
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public ExchangeRateSyncStatus Status { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public int RatesCount { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
