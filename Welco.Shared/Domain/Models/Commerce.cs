using Welco.Shared.Common.Classes;
namespace Welco.Shared.Domain.Models
{
    public enum OrderStatus { Pending = 1, Confirmed = 2, Shipped = 3, Delivered = 4, Cancelled = 5 }
    public enum InvoiceStatus { Draft = 1, Issued = 2, Paid = 3, Overdue = 4, Cancelled = 5 }
    public class Cart : BaseEntity<Guid>
    {
        public Guid? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public string? SessionId { get; set; }
        public Guid? CurrencyId { get; set; }
        public virtual Currency? Currency { get; set; }
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
    public class CartItem : BaseEntity<Guid>
    {
        public Guid CartId { get; set; }
        public virtual Cart? Cart { get; set; }
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPriceSnapshot { get; set; }
    }
    public class Order : BaseEntity<Guid>
    {
        public string OrderNumber { get; set; } = null!;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public Guid? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; }
        public Guid? CurrencyId { get; set; }
        public virtual Currency? Currency { get; set; }
        public Guid? QuoteId { get; set; }
        public decimal TotalAmount { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
    public class OrderItem : BaseEntity<Guid>
    {
        public Guid OrderId { get; set; }
        public virtual Order? Order { get; set; }
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    public class Invoice : BaseEntity<Guid>
    {
        public string InvoiceNumber { get; set; } = null!;
        public Guid OrderId { get; set; }
        public virtual Order? Order { get; set; }
        public decimal Amount { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        public string? FileUrl { get; set; }
    }
}
