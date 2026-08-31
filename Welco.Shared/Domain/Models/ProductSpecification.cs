using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class ProductSpecification : BaseEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public string AttrName { get; set; } = null!;
        public string AttrValue { get; set; } = null!;
    }
}
