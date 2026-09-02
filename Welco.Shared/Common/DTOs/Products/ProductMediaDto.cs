namespace Welco.Shared.Common.DTOs.Products
{
    public class ProductMediaDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Type { get; set; } // 1=Image, 2=Video, 3=Document
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
