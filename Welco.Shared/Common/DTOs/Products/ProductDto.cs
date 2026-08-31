namespace Welco.Shared.Common.DTOs.Products
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? Specifications { get; set; }
        public string? ImageName { get; set; }
        public string? Material { get; set; }
        public decimal? LengthCm { get; set; }
        public Guid? CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CurrencySymbol { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryNameEn { get; set; }
        public string? CategoryNameAr { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
