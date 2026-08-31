using Welco.Shared.Common.Classes;
namespace Welco.Shared.Domain.Models
{
    public class ShippingMethod : BaseEntity<Guid>
    {
        public string Name { get; set; } = null!;
        public int EtaMinDays { get; set; }
        public int EtaMaxDays { get; set; }
        public virtual ICollection<ShippingRate> Rates { get; set; } = new List<ShippingRate>();
    }
}
