namespace Welco.Shared.Common.DTOs.Sales
{
    public class RFQDto
    {
        public Guid Id { get; set; }
        public string RFQNumber { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? AssignedSalesRepId { get; set; }
        public List<RFQItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
    public class RFQItemDto { public Guid Id { get; set; } public Guid RFQId { get; set; } public Guid ProductId { get; set; } public int Quantity { get; set; } public string? Notes { get; set; } }
    public class QuoteDto
    {
        public Guid Id { get; set; }
        public string QuoteNumber { get; set; } = string.Empty;
        public Guid? RFQId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ValidUntil { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<QuoteItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
    public class QuoteItemDto { public Guid Id { get; set; } public Guid QuoteId { get; set; } public Guid ProductId { get; set; } public int Quantity { get; set; } public decimal UnitPrice { get; set; } }
}
