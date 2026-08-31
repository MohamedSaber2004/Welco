using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public enum ProductMediaType
    {
        Image = 1,
        Video = 2,
        Document = 3
    }

    public class ProductMedia : BaseEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public ProductMediaType Type { get; set; }
        public string Url { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
