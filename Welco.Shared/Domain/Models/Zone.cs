using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class Zone : BaseEntity<Guid>
    {
        public Guid CityId { get; set; }
        public virtual City City { get; set; } = null!;

        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;

        public static Zone Create(Guid cityId, string nameEn, string nameAr, string createdBy)
        {
            var zone = new Zone
            {
                Id = Guid.NewGuid(),
                CityId = cityId,
                NameEn = nameEn,
                NameAr = nameAr
            };
            zone.MarkAsCreated(createdBy);
            return zone;
        }

        public void Update(Guid? cityId, string? nameEn, string? nameAr, string updatedBy)
        {
            if (cityId.HasValue) CityId = cityId.Value;
            if (!string.IsNullOrWhiteSpace(nameEn)) NameEn = nameEn.Trim();
            if (!string.IsNullOrWhiteSpace(nameAr)) NameAr = nameAr.Trim();
            MarkAsUpdated(updatedBy);
        }
    }
}
