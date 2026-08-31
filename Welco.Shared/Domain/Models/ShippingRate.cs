using Welco.Shared.Common.Classes;
namespace Welco.Shared.Domain.Models
{
    public class ShippingRate : BaseEntity<Guid>
    {
        public Guid ShippingMethodId { get; set; }
        public virtual ShippingMethod? ShippingMethod { get; set; }
        public Guid DestinationCountryId { get; set; }
        public virtual Country? DestinationCountry { get; set; }
        public decimal BaseRate { get; set; }
    }
}
