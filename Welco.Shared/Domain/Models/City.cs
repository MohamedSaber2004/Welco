using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class City : BaseEntity<Guid>
    {
        public Guid CountryId { get; set; }
        public virtual Country Country { get; set; } = null!;

        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public virtual ICollection<Zone> Zones { get; set; } = new List<Zone>();

        public static City Create(Guid countryId, string nameEn, string nameAr, string createdBy)
        {
            var city = new City
            {
                Id = Guid.NewGuid(),
                CountryId = countryId,
                NameEn = nameEn,
                NameAr = nameAr
            };
            city.MarkAsCreated(createdBy);
            return city;
        }

        public void Update(Guid? countryId, string? nameEn, string? nameAr, string updatedBy)
        {
            if (countryId.HasValue) CountryId = countryId.Value;
            if (!string.IsNullOrWhiteSpace(nameEn)) NameEn = nameEn.Trim();
            if (!string.IsNullOrWhiteSpace(nameAr)) NameAr = nameAr.Trim();
            MarkAsUpdated(updatedBy);
        }
    }
}
