namespace Welco.Shared.Common.DTOs.Products
{
    public class SupplierDto
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Logo { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool Verified { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string CityEn { get; set; } = string.Empty;
        public string CityAr { get; set; } = string.Empty;
        public string FulfillmentDays { get; set; } = string.Empty;
        public string Tint { get; set; } = "#0E7169";
        public int ProductCount { get; set; }
    }
}
