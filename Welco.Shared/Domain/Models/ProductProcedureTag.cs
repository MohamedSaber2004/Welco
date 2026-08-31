using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class ProductProcedureTag : BaseEntity<Guid>
    {
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public string Label { get; set; } = null!;
    }
}
