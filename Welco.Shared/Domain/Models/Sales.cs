using Welco.Shared.Common.Classes;
namespace Welco.Shared.Domain.Models
{
    public enum RFQStatus { Pending = 1, Quoted = 2, Ordered = 3, Cancelled = 4 }
    public enum QuoteStatus { Draft = 1, Sent = 2, Approved = 3, Declined = 4, Expired = 5 }
    public enum DistributorApplicationStatus { Pending = 1, Approved = 2, Rejected = 3 }
    public class RFQ : BaseEntity<Guid>
    {
        public string RFQNumber { get; set; } = null!;
        public Guid CompanyId { get; set; }
        public virtual Company? Company { get; set; }
        public RFQStatus Status { get; set; } = RFQStatus.Pending;
        public Guid? AssignedSalesRepId { get; set; }
        public virtual ApplicationUser? AssignedSalesRep { get; set; }
        public virtual ICollection<RFQItem> Items { get; set; } = new List<RFQItem>();
    }
    public class RFQItem : BaseEntity<Guid>
    {
        public Guid RFQId { get; set; }
        public virtual RFQ? RFQ { get; set; }
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
    public class Quote : BaseEntity<Guid>
    {
        public string QuoteNumber { get; set; } = null!;
        public Guid? RFQId { get; set; }
        public virtual RFQ? RFQ { get; set; }
        public decimal Amount { get; set; }
        public DateTime ValidUntil { get; set; }
        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
        public Guid CreatedBySalesRepId { get; set; }
        public virtual ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    }
    public class QuoteItem : BaseEntity<Guid>
    {
        public Guid QuoteId { get; set; }
        public virtual Quote? Quote { get; set; }
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    public class ProductInquiry : BaseEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public string Name { get; set; } = null!;
        public string Organization { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? Email { get; set; }
    }
    public class DistributorApplication : BaseEntity<Guid>
    {
        public string CompanyName { get; set; } = null!;
        public Guid CountryId { get; set; }
        public virtual Country? Country { get; set; }
        public string SalesVolumeBand { get; set; } = null!;
        public string? CategoryInterest { get; set; }
        public string? Website { get; set; }
        public string ContactPerson { get; set; } = null!;
        public string ContactEmail { get; set; } = null!;
        public string? Phone { get; set; }
        public DistributorApplicationStatus Status { get; set; } = DistributorApplicationStatus.Pending;
    }
}
