namespace Welco.Shared.Common.DTOs.Products
{
    public class ProductMediaDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Type { get; set; } 
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
