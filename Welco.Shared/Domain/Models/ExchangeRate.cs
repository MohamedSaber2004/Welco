using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class ExchangeRate : BaseEntity<Guid>
    {
        public Guid BaseCurrencyId { get; set; }
        public Guid TargetCurrencyId { get; set; }
        public decimal Rate { get; set; }
        public DateOnly RateDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; }

        public virtual Currency BaseCurrency { get; set; } = null!;
        public virtual Currency TargetCurrency { get; set; } = null!;
    }
}
