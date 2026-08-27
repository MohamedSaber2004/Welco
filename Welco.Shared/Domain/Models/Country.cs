using Welco.Shared.Common.Classes;

namespace Welco.Shared.Domain.Models
{
    public class Country : BaseEntity<Guid>
    {
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public string? Code { get; set; }
        public virtual ICollection<City> Cities { get; set; } = new List<City>();

        public static Country Create(string nameEn, string nameAr, string? code, string createdBy)
        {
            var country = new Country
            {
                Id = Guid.NewGuid(),
                NameEn = nameEn,
                NameAr = nameAr,
                Code = code
            };
            country.MarkAsCreated(createdBy);
            return country;
        }

        public void Update(string? nameEn, string? nameAr, string? code, string updatedBy)
        {
            if (!string.IsNullOrWhiteSpace(nameEn)) NameEn = nameEn.Trim();
            if (!string.IsNullOrWhiteSpace(nameAr)) NameAr = nameAr.Trim();
            if (code != null) Code = code.Trim();
            MarkAsUpdated(updatedBy);
        }
    }
}
