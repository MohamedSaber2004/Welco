namespace Welco.Shared.Common.DTOs.Commerce
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? SessionId { get; set; }
        public Guid? CurrencyId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductNameEn { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPriceSnapshot { get; set; }
    }
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? CurrencyId { get; set; }
        public Guid? IncotermId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductNameEn { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
